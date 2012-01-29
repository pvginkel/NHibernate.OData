using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;

namespace NHibernate.OData.Demo
{
    public class SQLiteDialectEx : SQLiteDialect
    {
        public SQLiteDialectEx()
        {
            RegisterFunction("replace", new StandardSafeSQLFunction("replace", NHibernateUtil.String, 3));
            RegisterFunction("round", new StandardSQLFunction("round"));
        }
    }
}
