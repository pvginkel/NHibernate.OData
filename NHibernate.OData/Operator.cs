using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    // Specification at 2.2.3.6.1.1.2
    // Operator precedence according to 5.1.1.8 at http://docs.oasis-open.org/odata/odata/v4.0/csprd01/part2-url-conventions/odata-v4.0-csprd01-part2-url-conventions.html#_Toc355091904
    internal enum Operator
    {
        // Unary
        Negative,
        Not,
        // Multiplicative
        Mul,
        Div,
        Mod,
        // Additive
        Add,
        Sub,
        // Relational and type testing
        Gt,
        Ge,
        Lt,
        Le,
        // Equality
        Eq,
        Ne,
        // Conditional AND
        And,
        // Conditional OR
        Or
    }
}
