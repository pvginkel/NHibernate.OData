using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Normalization
{
    [TestFixture]
    internal class DateParts : NormalizedTestFixture
    {
        [Test]
        public void Tests()
        {
            Verify("year(datetime'2000-01-02T3:04:05')", 2000);
            Verify("month(datetime'2000-01-02T3:04:05')", 1);
            Verify("day(datetime'2000-01-02T3:04:05')", 2);
            Verify("hour(datetime'2000-01-02T3:04:05')", 3);
            Verify("minute(datetime'2000-01-02T3:04:05')", 4);
            Verify("second(datetime'2000-01-02T3:04:05')", 5);
            Verify("year(null)", null);
            VerifyThrows("year(1m)");
        }
    }
}
