using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iesi.Collections.Generic;

namespace NHibernate.OData
{
    internal class ODataSessionFactoryContext
    {
        internal static ODataSessionFactoryContext Empty = new ODataSessionFactoryContext();

        public ISet<System.Type> MappedClasses { get; private set; }

        private ODataSessionFactoryContext()
        {
            MappedClasses = new ReadOnlySet<System.Type>(new HashSet<System.Type>());
        }

        public ODataSessionFactoryContext(ISessionFactory sessionFactory)
        {
            Require.NotNull(sessionFactory, "sessionFactory");

            MappedClasses = new ReadOnlySet<System.Type>(new HashSet<System.Type>(
                sessionFactory.GetAllClassMetadata().Values.Select(x => x.GetMappedClass(EntityMode.Poco))
            ));
        }
    }
}
