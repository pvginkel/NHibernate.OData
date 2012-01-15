using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal abstract class Parser
    {
        private readonly IList<Token> _tokens;
        private int _offset;

        protected Parser(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _tokens = new Lexer(source).ToList();

            if (_tokens.Count == 0)
                throw new ODataException(ErrorMessages.Parser_EmptySource);

            Count = _tokens.Count;
        }

        protected Token Current
        {
            get { return _tokens[_offset]; }
        }

        protected object CurrentLiteral
        {
            get
            {
                var literal = Current as LiteralToken;

                if (literal == null)
                    throw new ODataException(ErrorMessages.Parser_ExpectedLiteral);

                return literal.Value;
            }
        }

        protected string CurrentIdentifier
        {
            get
            {
                var identifier = Current as IdentifierToken;

                if (identifier == null)
                    throw new ODataException(ErrorMessages.Parser_ExpectedIdentifier);

                return identifier.Identifier;
            }
        }

        protected char CurrentSyntax
        {
            get
            {
                var syntax = Current as SyntaxToken;

                if (syntax == null)
                    throw new ODataException(ErrorMessages.Parser_ExpectedSyntax);

                return syntax.Syntax;
            }
        }

        protected int Count { get; private set; }

        protected bool AtStart
        {
            get { return _offset == 0; }
        }

        protected bool AtEnd
        {
            get { return _offset >= Count; }
        }

        protected Token Previous
        {
            get { return AtStart ? null : _tokens[_offset - 1]; }
        }

        protected Token Next
        {
            get { return _offset < Count - 1 ? _tokens[_offset + 1] : null; }
        }

        protected void MoveNext()
        {
            //if (AtEnd)
            //    throw new ODataException(ErrorMessages.Parser_UnexpectedEnd);

            _offset++;
        }

        protected void Expect(Token token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            if (AtEnd || !Equals(Current, token))
                throw new ODataException(String.Format(ErrorMessages.Parser_ExpectedToken, token));

            MoveNext();
        }

        public abstract Expression Parse();

        protected void ExpectAtEnd()
        {
            if (!AtEnd)
                throw new ODataException(ErrorMessages.Parser_TrailingTokens);
        }

        protected void ExpectAny()
        {
            if (AtEnd)
                throw new ODataException(ErrorMessages.Parser_UnexpectedEnd);
        }

        protected Expression ParseBool()
        {
            return ExpressionUtil.CoerceBoolExpression(ParseCommon());
        }

        protected Expression ParseCommon()
        {
            var result = ParseCommonItem();

            while (!(AtEnd || Current == SyntaxToken.ParenClose || Current == SyntaxToken.Comma))
            {
                var @operator = GetOperator(Current);

                MoveNext();

                ExpectAny();

                var right = ParseCommon();

                // Apply operator precedence

                var binary = right as BinaryExpression;

                if (binary != null && binary.Operator < @operator)
                {
                    result = CreateBinary(
                        binary.Operator,
                        CreateBinary(
                            @operator,
                            result,
                            binary.Left
                        ),
                        binary.Right
                    );
                }
                else
                {
                    result = CreateBinary(
                        @operator,
                        result,
                        right
                    );
                }
            }

            return result;
        }

        private Expression CreateBinary(Operator @operator, Expression left, Expression right)
        {
            if (OperatorUtil.IsLogical(@operator))
                return new LogicalExpression(@operator, left, right);
            else if (OperatorUtil.IsCompare(@operator))
                return new ComparisonExpression(@operator, left, right);
            else if (OperatorUtil.IsArithmic(@operator))
                return new ArithmicExpression(@operator, left, right);
            else
                throw new NotSupportedException();
        }

        private Expression ParseCommonItem()
        {
            switch (Current.Type)
            {
                case TokenType.Literal:
                    object value = CurrentLiteral;

                    MoveNext();

                    return new LiteralExpression(value);

                case TokenType.Syntax:
                    if (Current == SyntaxToken.Negative)
                    {
                        MoveNext();

                        return new ArithmicUnaryExpression(Operator.Negative, ParseCommonItem());
                    }
                    if (Current == SyntaxToken.ParenOpen)
                    {
                        MoveNext();

                        ExpectAny();

                        var result = new ParenExpression(ParseCommon());

                        Expect(SyntaxToken.ParenClose);

                        return result;
                    }
                    else
                    {
                        throw new ODataException(ErrorMessages.Parser_ExpectedSyntax);
                    }

                case TokenType.Identifier:
                    if (Next == SyntaxToken.ParenOpen)
                    {
                        var methodCall = ParseMethodCall();

                        MoveNext();

                        return methodCall;
                    }
                    else if (CurrentIdentifier == "not")
                    {
                        MoveNext();

                        return new BoolUnaryExpression(Operator.Not, ParseCommonItem());
                    }
                    else
                    {
                        var members = new List<string>();

                        members.Add(CurrentIdentifier);

                        MoveNext();

                        while (!AtEnd && Current == SyntaxToken.Slash)
                        {
                            MoveNext();

                            ExpectAny();

                            members.Add(CurrentIdentifier);

                            MoveNext();
                        }

                        return new MemberExpression(MemberType.Normal, members);
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private Operator GetOperator(Token token)
        {
            var identifier = token as IdentifierToken;

            if (identifier != null)
            {
                switch (identifier.Identifier)
                {
                    case "and": return Operator.And;
                    case "or": return Operator.Or;
                    case "eq": return Operator.Eq;
                    case "ne": return Operator.Ne;
                    case "lt": return Operator.Lt;
                    case "le": return Operator.Le;
                    case "gt": return Operator.Gt;
                    case "ge": return Operator.Ge;
                    case "add": return Operator.Add;
                    case "sub": return Operator.Sub;
                    case "mul": return Operator.Mul;
                    case "div": return Operator.Div;
                    case "mod": return Operator.Mod;
                }
            }

            throw new ODataException(ErrorMessages.Parser_ExpectedOperator);
        }

        private MethodCallExpression ParseMethodCall()
        {
            throw new NotImplementedException();
        }
    }
}
