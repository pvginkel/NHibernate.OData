using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.OData.Test.Domain;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using NhEnvironment = NHibernate.Cfg.Environment;

namespace NHibernate.OData.Test.Support
{
    internal class DomainTestFixture
    {
        private string _databasePath;
        private string _databaseBackupPath;
        private ISessionFactory _sessionFactory;
        private ISession _testSession;

        protected ISession Session
        {
            get
            {
                if (_testSession == null)
                    _testSession = OpenSession();

                return _testSession;
            }
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            _databasePath = Path.GetTempFileName();
            _databaseBackupPath = Path.GetTempFileName();
            
            var cfg = new Configuration()
                .SetProperty(NhEnvironment.Dialect, typeof(SQLiteDialectEx).AssemblyQualifiedName)
                .SetProperty(NhEnvironment.ConnectionString, String.Format("data source={0};pooling=false;", _databasePath))
                .SetProperty(NhEnvironment.ShowSql, "true")
                .AddAssembly(GetType().Assembly);

            new SchemaExport(cfg).Execute(false, true, false);

            _sessionFactory = cfg.BuildSessionFactory();

            PopulateDatabase();

            File.Copy(_databasePath, _databaseBackupPath, true);
        }

        private void PopulateDatabase()
        {
            using (var session = OpenSession())
            {
                string lengthString = "";

                for (int i = 1; i <= 10; i++)
                {
                    lengthString += ((char)('A' + (i - 1))).ToString();

                    var child = new Child
                    {
                        Name = "Child " + i,
                        Int32 = i
                    };

                    session.Save(child);

                    session.Save(new Parent
                    {
                        Name = "Parent " + i,
                        Int32 = i,
                        Child = child,
                        LengthString = lengthString,
                        DateTime = new DateTime(2000 + i, i, i, i, i, i)
                    });
                }
            }
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            _sessionFactory.Dispose();
            _sessionFactory = null;

            File.Delete(_databasePath);
            File.Delete(_databaseBackupPath);
        }

        [SetUp]
        public void SetUp()
        {
            File.Copy(_databaseBackupPath, _databasePath, true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_testSession != null)
            {
                _testSession.Dispose();
                _testSession = null;
            }
        }

        protected ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        protected void Verify<T>(string filter, IList<T> expected)
            where T : class, IEntity
        {
            Verify(filter, expected, false);
        }

        protected void Verify<T>(string filter, IList<T> expected, bool ordered)
            where T : class, IEntity
        {
            var actual = Session.ODataQuery<T>(GetQueryString(filter)).List<T>();

            if (actual.Count == 0)
                throw new InvalidOperationException("Query returned zero results");

            if (ordered)
            {
                Assert.That(actual, Is.EqualTo(expected));
            }
            else
            {
                Assert.That(actual, Is.EquivalentTo(expected));
            }
        }

        protected void VerifyEmpty<T>(string filter)
            where T : class, IEntity
        {
            var actual = Session.ODataQuery<T>(GetQueryString(filter)).List<T>();

            Assert.AreEqual(0, actual.Count);
        }

        protected void Verify<T>(string filter, Func<IQueryOver<T, T>, IQueryOver<T, T>> query)
            where T : class, IEntity
        {
            Verify(filter, query(Session.QueryOver<T>()).List());
        }

        protected void VerifyOrdered<T>(string filter, Func<IQueryOver<T, T>, IQueryOver<T, T>> query)
            where T : class, IEntity
        {
            Verify(filter, query(Session.QueryOver<T>()).List(), true);
        }

        protected void Verify<T>(string filter, ICriterion criterion)
            where T : class, IEntity
        {
            Verify(filter, Session.CreateCriteria<T>().Add(criterion).List<T>());
        }

        protected void VerifyThrows<T>(string filter)
        {
            VerifyThrows<T>(filter, typeof(QueryNotSupportException));
        }

        protected void VerifyThrows<T>(string filter, System.Type exceptionType)
        {
            try
            {
                Session.ODataQuery<T>(GetQueryString(filter)).List<T>();

                Assert.Fail("Expected exception");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionType, ex.GetType());
            }
        }

        private string GetQueryString(string filter)
        {
            if (filter.Length == 0 || filter[0] == '$')
                return filter;
            else
                return "$filter=" + Uri.EscapeDataString(filter);
        }
    }
}
