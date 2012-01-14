using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// The Equals are only here for unit testing. GetHashCode has not been implemented.

#pragma warning disable 659

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

    internal class IdentifierToken : Token
    {
        public string Identifier { get; private set; }

        public IdentifierToken(string name)
            : base(TokenType.Identifier)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Identifier = name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as IdentifierToken;

            return other != null && Identifier == other.Identifier;
        }

        public override string ToString()
        {
            return String.Format("\"{0}\"", Identifier);
        }
    }

    internal class LiteralToken : Token
    {
        public static readonly LiteralToken PositiveInfinity = new LiteralToken(double.PositiveInfinity);
        public static readonly LiteralToken NegativeInfinity = new LiteralToken(double.NegativeInfinity);
        public static readonly LiteralToken NaN = new LiteralToken(double.NaN);
        public static readonly LiteralToken True = new LiteralToken(true);
        public static readonly LiteralToken False = new LiteralToken(false);
        public static readonly LiteralToken Null = new LiteralToken(null);

        public object Value { get; private set; }

        public LiteralToken(object value)
            : base(TokenType.Literal)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as LiteralToken;

            if (other == null)
                return false;

            var bytes = Value as byte[];
            var otherBytes = other.Value as byte[];

            if (bytes != null && otherBytes != null)
            {
                if (bytes.Length != otherBytes.Length)
                    return false;

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != otherBytes[i])
                        return false;
                }

                return true;
            }

            return Equals(Value, other.Value);
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "null";
        }
    }

    internal class SyntaxToken : Token
    {
        public static readonly SyntaxToken ParenOpen = new SyntaxToken('(');
        public static readonly SyntaxToken ParenClose = new SyntaxToken(')');
        public static readonly SyntaxToken Slash = new SyntaxToken('/');
        public static readonly SyntaxToken Comma = new SyntaxToken(',');
        public static readonly SyntaxToken Negative = new SyntaxToken('-');

        public char Syntax { get; private set; }

        private SyntaxToken(char syntax)
            : base(TokenType.Syntax)
        {
            Syntax = syntax;
        }

        public override string ToString()
        {
            return String.Format("'{0}'", Syntax);
        }
    }
}
