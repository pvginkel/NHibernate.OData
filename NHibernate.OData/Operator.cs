using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    // Precedence taken from http://msdn.microsoft.com/en-us/library/Aa691323
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
        Le,
        Gt,
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