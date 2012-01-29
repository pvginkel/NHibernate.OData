using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NHibernate.Criterion;
using NHibernate.OData.Demo.Domain;

namespace NHibernate.OData.Demo
{
    internal class ODataRequest
    {
        private static readonly XNamespace NsAtom = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace NsApp = "http://www.w3.org/2007/app";
        private static readonly XNamespace NsEdm = "http://schemas.microsoft.com/ado/2007/05/edm";
        private static readonly XNamespace NsEdmx = "http://schemas.microsoft.com/ado/2007/06/edmx";
        private static readonly XNamespace NsMetadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private static readonly XNamespace NsDataServices = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        private static readonly object _syncRoot = new object();
        private static List<System.Type> _types;

        private readonly Database _database;
        private readonly ISession _session;
        private readonly string _path;
        private readonly string _filter;
        private readonly XNamespace _namespace;

        public string DataServiceVersion { get; private set; }

        public ODataRequest(Database database, ISession session, string path, string filter, XNamespace @namespace)
        {
            if (database == null)
                throw new ArgumentNullException("database");
            if (session == null)
                throw new ArgumentNullException("session");
            if (path == null)
                throw new ArgumentNullException("path");

            _database = database;
            _session = session;
            _path = path;
            _filter = filter;
            _namespace = @namespace;
        }

        public XDocument GetResponse()
        {
            string path = _path.Trim('/');

            DataServiceVersion = "1.0;";

            if (path.Length == 0)
                return GetServiceDescriptionResponse();
            else if (path == "$metadata")
                return GetMetadataResponse();
            else
                return GetDataResponse(path);
        }

        private XDocument GetServiceDescriptionResponse()
        {
            var workspaceElement = new XElement(
                NsApp + "workspace",
                new XElement(NsAtom + "title", "Default")
            );

            foreach (var type in GetTypes())
            {
                string name = Inflector.Pluralize(type.Name);

                workspaceElement.Add(new XElement(
                    NsApp + "collection",
                    new XAttribute("href", name),
                    new XElement(
                        NsAtom + "title",
                        name
                    )
                ));
            }

            return new XDocument(
                new XElement(
                    NsApp + "service",
                    new XAttribute(XNamespace.Xmlns + "atom", NsAtom),
                    new XAttribute(XNamespace.Xmlns + "app", NsApp),
                    new XAttribute("xmlns", NsApp),
                    new XAttribute(XNamespace.Xml + "base", _namespace),
                    workspaceElement
                )
            );
        }

        private XDocument GetMetadataResponse()
        {
            var schemaElement = new XElement(
                NsEdm + "Schema",
                new XAttribute(XNamespace.Xmlns + "d", NsDataServices),
                new XAttribute(XNamespace.Xmlns + "m", NsMetadata),
                new XAttribute("xmlns", NsEdm),
                new XAttribute("Namespace", "SouthWind")
            );

            var entityContainerElement = new XElement(
                NsEdm + "EntityContainer",
                new XAttribute("Name", "NHibernate"),
                new XAttribute(NsMetadata + "IsDefaultEntityContainer", "true")
            );

            var associations = new Dictionary<string, XElement>();

            foreach (var type in GetTypes())
            {
                var entityElement = new XElement(
                    NsEdm + "EntityType",
                    new XAttribute("Name", type.Name),
                    new XElement(
                        NsEdm + "Key",
                        new XElement(
                            NsEdm + "PropertyRef",
                            new XAttribute("Name", "Id")
                        )
                    )
                );

                schemaElement.Add(entityElement);

                foreach (var property in type.GetProperties())
                {
                    bool isEntitySet = IsEntitySet(property.PropertyType);

                    if (
                        isEntitySet ||
                        typeof(IEntity).IsAssignableFrom(property.PropertyType)
                    ) {
                        var propertyType = property.PropertyType;

                        if (isEntitySet)
                            propertyType = propertyType.GetGenericArguments()[0];

                        string fromRole;
                        string toRole;
                        string relationship;
                        string multiplicity;

                        if (isEntitySet)
                        {
                            fromRole = type.Name + "_" + property.Name;
                            toRole = propertyType.Name + "_" + type.Name;
                            relationship = toRole + "_" + fromRole;
                            multiplicity = "*";
                        }
                        else
                        {
                            fromRole = type.Name + "_" + property.Name;
                            toRole = propertyType.Name + "_" + Inflector.Pluralize(type.Name);
                            relationship = fromRole + "_" + toRole;
                            multiplicity = "0..1";
                        }

                        entityElement.Add(new XElement(
                            NsEdm + "NavigationProperty",
                            new XAttribute("Name", property.Name),
                            new XAttribute("Relationship", "SouthWind." + relationship),
                            new XAttribute("FromRole", fromRole),
                            new XAttribute("ToRole", toRole)
                        ));

                        XElement association;

                        if (!associations.TryGetValue(relationship, out association))
                        {
                            association = new XElement(
                                NsEdm + "Association",
                                new XAttribute("Name", relationship)
                            );

                            associations.Add(relationship, association);
                        }

                        association.Add(new XElement(
                            NsEdm + "End",
                            new XAttribute("Role", toRole),
                            new XAttribute("Type", "SouthWind." + propertyType.Name),
                            new XAttribute("Multiplicity", multiplicity)
                        ));
                    }
                    else
                    {
                        entityElement.Add(new XElement(
                            NsEdm + "Property",
                            new XAttribute("Name", property.Name),
                            new XAttribute("Type", GetEdmType(property.PropertyType)),
                            new XAttribute("Nullable", GetNullable(property.PropertyType))
                        ));
                    }
                }

                entityContainerElement.Add(new XElement(
                    NsEdm + "EntitySet",
                    new XAttribute("Name", Inflector.Pluralize(type.Name)),
                    new XAttribute("EntityType", "SouthWind." + type.Name)
                ));
            }

            foreach (var association in associations.Values)
            {
                schemaElement.Add(association);
            }

            schemaElement.Add(entityContainerElement);

            return new XDocument(
                new XElement(
                    NsEdmx + "Edmx",
                    new XAttribute(XNamespace.Xmlns + "edmx", NsEdmx),
                    new XAttribute("Version", "1.0"),
                    new XElement(
                        NsEdmx + "DataServices",
                        new XAttribute(XNamespace.Xmlns + "m", NsMetadata),
                        new XAttribute(NsMetadata + "DataServiceVersion", "2.0"),
                        schemaElement
                    )
                )
            );
        }

        private XDocument GetDataResponse(string path)
        {
            DataServiceVersion = "2.0;";

            string entityName;

            string[] parts = path.Split('/');

            if (parts.Length > 2)
                throw new NotSupportedException();

            entityName = Inflector.Singularize(parts[parts.Length - 1]);
            var builder = _database.GetBuilder(entityName);

            ICriteria criteria;

            if (String.IsNullOrEmpty(_filter))
                criteria = _session.CreateCriteria(entityName);
            else
                criteria = _session.ODataQuery(entityName, _filter);

            if (parts.Length == 2)
            {
                var match = Regex.Match(parts[0], "^(.*?)\\((.*?)\\)$");

                if (!match.Success)
                    throw new InvalidOperationException();

                string parentEntityName = Inflector.Singularize(match.Groups[1].Value);
                var parentBuilder = _database.GetBuilder(parentEntityName);
                object parentId = new IdValueParser(match.Groups[2].Value).Parse();
                var parentEntity = _session.Load(parentEntityName, parentId);

                if (IsEntitySet(parentBuilder.Properties[parts[1]].PropertyType))
                {
                    criteria.Add(Restrictions.Eq(parentEntityName, parentEntity));
                }
                else
                {
                    var childEntity = parentBuilder.Accessor[parentEntity, parts[1]];
                    var childBuilder = _database.GetBuilder(parts[1]);

                    criteria.Add(Restrictions.Eq("Id", childBuilder.Accessor[childEntity, "Id"]));
                }
            }

            var feedElement = new XElement(
                NsAtom + "feed",
                new XAttribute(XNamespace.Xml + "base", _namespace),
                new XAttribute(XNamespace.Xmlns + "d", NsDataServices),
                new XAttribute(XNamespace.Xmlns + "m", NsMetadata),
                new XAttribute("xmlns", NsAtom),
                new XElement(
                    NsAtom + "title",
                    new XAttribute("type", "text"),
                    path
                ),
                new XElement(
                    NsAtom + "id",
                    _namespace.ToString().TrimEnd('/') + "/" + path
                ),
                new XElement(
                    NsAtom + "updated",
                    DateTime.UtcNow.ToString("s") + "Z"
                ),
                new XElement(
                    NsAtom + "link",
                    new XAttribute("rel", "self"),
                    new XAttribute("title", path),
                    new XAttribute("href", path)
                )
            );

            foreach (var entity in criteria.List<IEntity>())
            {
                var propertiesElement = new XElement(
                    NsMetadata + "properties"
                );

                string id = entityName + "(" + GetIdValue(builder.Accessor[entity, "Id"], builder.Properties["Id"].PropertyType) + ")";

                var entryElement = new XElement(
                    NsAtom + "entry",
                    new XElement(
                        NsAtom + "id",
                        _namespace.ToString().TrimEnd('/') + "/" + id
                    ),
                    new XElement(
                        NsAtom + "author",
                        new XElement(NsAtom + "name")
                    )
                );

                feedElement.Add(entryElement);

                foreach (var property in builder.Properties)
                {
                    if (
                        IsEntitySet(property.Value.PropertyType) ||
                        typeof(IEntity).IsAssignableFrom(property.Value.PropertyType)
                    ) {
                        entryElement.Add(new XElement(
                            NsAtom + "link",
                            new XAttribute("rel", NsDataServices.ToString().TrimEnd('/') + "/related/" + property.Key),
                            new XAttribute("type", "application/atom+xml;type=entry"),
                            new XAttribute("title", property.Key),
                            new XAttribute("href", id + "/" + property.Key)
                        ));
                    }
                    else
                    {
                        propertiesElement.Add(
                            new XElement(
                                NsDataServices + property.Key,
                                new XAttribute(NsMetadata + "type", GetEdmType(property.Value.PropertyType)),
                                GetValue(builder.Accessor[entity, property.Key], property.Value.PropertyType)
                            )
                        );
                    }
                }

                entryElement.Add(
                    new XElement(
                        NsAtom + "category",
                        new XAttribute("term", "SouthWind." + entityName),
                        new XAttribute("schema", NsDataServices)
                    )
                );

                entryElement.Add(
                    new XElement(
                        NsAtom + "content",
                        new XAttribute("type", "application/xml"),
                        propertiesElement
                    )
                );
            }

            return new XDocument(feedElement);
        }

        private string GetIdValue(object value, System.Type type)
        {
            if (type == typeof(int))
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(string))
                return "'" + ((string)value).Replace("'", "''") + "'";
            else
                throw new NotSupportedException();
        }

        private string GetValue(object value, System.Type type)
        {
            if (type == typeof(string))
                return (string)value;
            else if (type == typeof(DateTime))
                return ((DateTime)value).ToString("s");
            else if (type == typeof(int))
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);
            else if (type == typeof(bool))
                return (bool)value ? "true" : "false";
            else if (type == typeof(byte[]))
                return Convert.ToBase64String((byte[])value);
            else
                throw new NotSupportedException();
        }

        private bool IsEntitySet(System.Type type)
        {
            if (!type.IsGenericType)
                return false;

            var arguments = type.GetGenericArguments();

            return arguments.Length == 1 && typeof(IEntity).IsAssignableFrom(arguments[0]);
        }

        private string GetEdmType(System.Type type)
        {
            if (type == typeof(string))
                return "Edm.String";
            else if (type == typeof(DateTime))
                return "Edm.DateTime";
            else if (type == typeof(int))
                return "Edm.Int32";
            else if (type == typeof(decimal))
                return "Edm.Decimal";
            else if (type == typeof(bool))
                return "Edm.Boolean";
            else if (type == typeof(byte[]))
                return "Edm.Binary";
            else
                throw new NotSupportedException();
        }

        private object GetNullable(System.Type type)
        {
            if (type.IsClass)
                return "true";
            else
                return "false";
        }

        private static IEnumerable<System.Type> GetTypes()
        {
            lock (_syncRoot)
            {
                if (_types == null)
                {
                    _types = new List<System.Type>(
                        from type in typeof(ODataRequest).Assembly.GetTypes()
                        where type != typeof(IEntity) && type.Namespace == typeof(ODataRequest).Namespace + ".Domain"
                        select type
                    );
                }

                return _types;
            }
        }
    }
}
