using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    internal enum ExpressionType
    {
        Literal,
        Bool,
        Arithmic,
        Not,
        IsOf,
        BoolCast,
        MethodCall,
        Negative,
        Member,
        Paren,
        ArithmicUnary
    }
}