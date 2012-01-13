using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal abstract class Token
    {
        protected Token(TokenType type)
        {
            Type = type;
        }

        public TokenType Type { get; private set; }
    }

    internal class NameToken : Token
    {
        public string Name { get; private set; }

        public NameToken(string name)
            : base(TokenType.Name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
        }
    }

    internal class LiteralToken : Token
    {
        public object Value { get; private set; }

        public LiteralToken(object value)
            : base(TokenType.Literal)
        {
            Value = value;
        }
    }

    internal class SyntaxToken : Token
    {
        public char Syntax { get; private set; }

        public SyntaxToken(char syntax)
            : base(TokenType.Syntax)
        {
            Syntax = syntax;
        }
    }
}
