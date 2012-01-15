using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    // Specification at 2.2.3.6.1.1.2
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
        Lt,
        Gt,
        Le,
        Ge,
        // Equality
        Eq,
        Ne,
        // Conditional AND
        And,
        // Conditional OR
        Or
    }
}