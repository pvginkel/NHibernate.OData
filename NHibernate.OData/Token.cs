using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Require.NotNull(name, "name");

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
        public static readonly LiteralToken PositiveInfinity = new LiteralToken(double.PositiveInfinity, LiteralType.Double);
        public static readonly LiteralToken NegativeInfinity = new LiteralToken(double.NegativeInfinity, LiteralType.Double);
        public static readonly LiteralToken NaN = new LiteralToken(double.NaN, LiteralType.Double);
        public static readonly LiteralToken True = new LiteralToken(true, LiteralType.Boolean);
        public static readonly LiteralToken False = new LiteralToken(false, LiteralType.Boolean);
        public static readonly LiteralToken Null = new LiteralToken(null, LiteralType.Null);

        public object Value { get; private set; }
        public LiteralType LiteralType { get; private set; }

        public LiteralToken(object value)
            : this(value, LiteralUtil.GetLiteralType(value))
        {
            // This overload is here just for unit testing.
        }

        public LiteralToken(object value, LiteralType literalType)
            : base(TokenType.Literal)
        {
            Value = value;
            LiteralType = literalType;

            Debug.Assert(literalType == LiteralUtil.GetLiteralType(value));
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
                return LiteralUtil.ByteArrayEquals(bytes, otherBytes);

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
