using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NHibernate.Cfg;
using NHibernate.OData.Demo.Populator;
using NHibernate.Tool.hbm2ddl;
using Environment = NHibernate.Cfg.Environment;
using NHibernate.OData.Demo.Domain;

namespace NHibernate.OData.Demo
{
    internal class Database : IDisposable
    {
        private const string DatabasePath = "SouthWind.db";

        public ISessionFactory SessionFactory { get; private set; }

        private bool _disposed;

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

                new DatabasePopulator(this).Populate();
            }

            Console.WriteLine(" done");
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
