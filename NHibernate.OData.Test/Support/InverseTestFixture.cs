using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Test.Support
{
    internal abstract class InverseTestFixture : ParserTestFixture
    {
        protected override void Verify(Expression actual, Expression expected)
        {
            base.Verify(InverseVisitor.Invert(actual), expected);
        }

        protected override Expression VerifyThrows(Expression expression)
        {
            return InverseVisitor.Invert(expression);
        }
    }
}
