using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Environment = NHibernate.Cfg.Environment;
using NHibernate.OData.Demo.Domain;

namespace NHibernate.OData.Demo
{
    internal class Database : IDisposable
    {
        private const string DatabasePath = "SouthWind.db";

        private static readonly string[] BuilderOder =
        {
            "Category",
            "Customer",
            "Region",
            "Territory",
            "Employee",
            "Supplier",
            "Product",
            "Order",
            "Shipper"
        };

        private static readonly Dictionary<string, string> NameMap = new Dictionary<string,string>
        {
            { "EmployeeTerritorie", "EmployeeTerritory" }
        };

        public ISessionFactory SessionFactory { get; private set; }

        private bool _disposed;
        private readonly Dictionary<string, EntityBuilder> _builders = new Dictionary<string, EntityBuilder>();
        private readonly Dictionary<System.Type, EntityBuilder> _buildersByType = new Dictionary<System.Type, EntityBuilder>();
        private Dictionary<System.Type, Dictionary<object, object>> _entityCache = new Dictionary<System.Type, Dictionary<object, object>>();
        private readonly object _syncRoot = new object();

        public Database()
        {
            Console.Write("Setting up NHibernate...");

            bool exists = File.Exists(DatabasePath);

            var cfg = new Configuration()
                .SetProperty(Environment.Dialect, typeof(SQLiteDialectEx).AssemblyQualifiedName)
                .SetProperty(Environment.ConnectionString, String.Format("data source={0}", DatabasePath))
                .SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote")
                // .SetProperty(Environment.ShowSql, "true")
                .AddAssembly(GetType().Assembly);

            if (!exists)
                new SchemaExport(cfg).Execute(false, true, false);

            SessionFactory = cfg.BuildSessionFactory();

            if (!exists)
            {
                Console.Write(" populating database...");

                Populate();
            }

            Console.WriteLine(" done");
        }

        private void Populate()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".SouthWind.xml"))
            using (var reader = XmlReader.Create(stream))
            using (var session = OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.FlushMode = FlushMode.Never;

                var document = XDocument.Load(reader);

                foreach (string entityName in BuilderOder)
                {
                    foreach (var element in document.Root.Elements().SelectMany(p => p.Elements(entityName)))
                    {
                        var entity = LoadEntity(element);

                        session.Save(entity);

                        foreach (var property in element.Elements())
                        {
                            foreach (var childElement in property.Elements())
                            {
                                var childEntity = LoadEntity(childElement);

                                var builder = _buildersByType[childEntity.GetType()];

                                builder.Accessor[childEntity, entityName] = entity;

                                session.Save(childEntity);
                            }
                        }
                    }
                }

                session.Flush();
                transaction.Commit();
            }

            _entityCache = null;
        }

        private object LoadEntity(XElement element)
        {
            string entityName;

            if (!NameMap.TryGetValue(element.Name.LocalName, out entityName))
                entityName = element.Name.LocalName;

            var builder = GetBuilder(entityName);

            var obj = builder.CreateInstance();

            var idAttribute = element.Attribute(element.Name.LocalName + "ID");

            if (idAttribute != null)
            {
                var id = GetValue(
                    idAttribute.Value,
                    builder.Properties["Id"].PropertyType
                );

                builder.Accessor[obj, "Id"] = id;

                CacheEntity(obj, id);
            }

            foreach (var property in element.Elements())
            {
                if (property.Elements().Any())
                    continue;

                string propertyName = property.Name.LocalName;

                if (propertyName.EndsWith("ID"))
                    propertyName = propertyName.Substring(0, propertyName.Length - 2);

                builder.Accessor[obj, propertyName] = GetValue(
                    property.Value,
                    builder.Properties[propertyName].PropertyType
                );
            }

            return obj;
        }

        private void CacheEntity(object obj, object id)
        {
            Dictionary<object, object> entities;

            if (!_entityCache.TryGetValue(obj.GetType(), out entities))
            {
                entities = new Dictionary<object, object>();

                _entityCache.Add(obj.GetType(), entities);
            }

            entities[id] = obj;
        }

        public EntityBuilder GetBuilder(string entityName)
        {
            lock (_syncRoot)
            {
                EntityBuilder builder;

                if (!_builders.TryGetValue(entityName, out builder))
                {
                    var type = GetType().Assembly.GetType(GetType().Namespace + ".Domain." + entityName);

                    builder = new EntityBuilder(type);

                    _builders.Add(entityName, builder);
                    _buildersByType.Add(type, builder);
                }

                return builder;
            }
        }

        private object GetValue(string value, System.Type type)
        {
            if (type == typeof(string))
                return value;
            else if (type == typeof(DateTime))
                return DateTime.Parse(value);
            else if (type == typeof(int))
                return int.Parse(value);
            else if (type == typeof(decimal))
                return decimal.Parse(value);
            else if (type == typeof(bool))
                return value == "true";
            else if (type == typeof(byte[]))
                return Convert.FromBase64String(value);
            else if (typeof(IEntity).IsAssignableFrom(type))
                return GetEntity(value, type);
            else
                throw new NotSupportedException();
        }

        private object GetEntity(string value, System.Type type)
        {
            var builder = _buildersByType[type];

            object id = GetValue(value, builder.Properties["Id"].PropertyType);

            return _entityCache[type][id];
        }

        public ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (SessionFactory != null)
                {
                    SessionFactory.Dispose();
                    SessionFactory = null;
                }

                _disposed = true;
            }
        }
    }
}
