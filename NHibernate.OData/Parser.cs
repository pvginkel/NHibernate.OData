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
        private readonly ParserMode _mode;

        protected Parser(string source, ParserMode mode)
        {
            Require.NotNull(source, "source");

            _mode = mode;

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
            Require.NotNull(token, "token");

            if (AtEnd || !Equals(Current, token))
                throw new ODataException(String.Format(ErrorMessages.Parser_ExpectedToken, token));

            MoveNext();
        }

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
            return ParseCommon(ParseCommonItem());
        }

        protected Expression ParseCommon(Expression result)
        {
            while (!(AtEnd || Current == SyntaxToken.ParenClose || Current == SyntaxToken.Comma || GetOrderByDirection(Current).HasValue))
            {
                var @operator = GetOperator(Current);

                if (!@operator.HasValue)
                    throw new ODataException(ErrorMessages.Parser_ExpectedOperator);
                else if (!OperatorUtil.IsBinary(@operator.Value))
                    throw new ODataException(ErrorMessages.Parser_ExpectedBinaryOperator);

                MoveNext();

                ExpectAny();

                var right = ParseCommonItem();

                // Apply operator precedence

                var binary = result as BinaryExpression;

                if (binary != null && binary.Operator < @operator.Value)
                {
                    result = CreateBinary(
                        @operator.Value,
                        result,
                        ParseCommon(right)
                    );
                }
                else
                {
                    result = CreateBinary(
                        @operator.Value,
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
            else if (OperatorUtil.IsArithmetic(@operator))
                return new ArithmeticExpression(@operator, left, right);
            else
                throw new NotSupportedException();
        }

        private Expression ParseCommonItem()
        {
            switch (Current.Type)
            {
                case TokenType.Literal:
                    var literal = (LiteralToken)Current;

                    MoveNext();

                    return new LiteralExpression(literal.Value, literal.LiteralType);

                case TokenType.Syntax:
                    if (Current == SyntaxToken.Negative)
                    {
                        MoveNext();

                        return new ArithmeticUnaryExpression(Operator.Negative, ParseCommonItem());
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
                    if (Next == SyntaxToken.ParenOpen && !GetOperator(Current).HasValue && _mode != ParserMode.Path)
                    {
                        return ParseMethodCall();
                    }
                    else if (GetOperator(Current) == Operator.Not)
                    {
                        MoveNext();

                        return new BoolUnaryExpression(Operator.Not, ParseCommonItem());
                    }
                    else
                    {
                        var members = new List<MemberExpressionComponent>();

                        ParseMember(members);

                        while (!AtEnd && Current == SyntaxToken.Slash)
                        {
                            MoveNext();

                            ExpectAny();

                            ParseMember(members);
                        }

                        return new MemberExpression(MemberType.Normal, members);
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private void ParseMember(List<MemberExpressionComponent> members)
        {
            string name = CurrentIdentifier;
            LiteralExpression idExpression = null;

            MoveNext();

            if (!AtEnd && _mode == ParserMode.Path && Current == SyntaxToken.ParenOpen)
            {
                MoveNext();

                idExpression = ParseCommon() as LiteralExpression;

                if (idExpression == null)
                    throw new ODataException(ErrorMessages.Parser_ExpectedLiteralExpression);

                Expect(SyntaxToken.ParenClose);
            }

            members.Add(new MemberExpressionComponent(name, idExpression));
        }

        private Operator? GetOperator(Token token)
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
                    case "not": return Operator.Not;
                }
            }

            return null;
        }

        protected OrderByDirection? GetOrderByDirection(Token token)
        {
            var identifier = token as IdentifierToken;

            if (identifier != null)
            {
                switch (identifier.Identifier)
                {
                    case "asc": return OrderByDirection.Ascending;
                    case "desc": return OrderByDirection.Descending;
                }
            }

            return null;
        }

        private MethodCallExpression ParseMethodCall()
        {
            var method = Method.FindMethodByName(CurrentIdentifier);

            if (method == null)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Parser_UnknownMethod, CurrentIdentifier
                ));
            }

            MoveNext();

            Expect(SyntaxToken.ParenOpen);

            var arguments = ParseMethodCallArgumentList(method);

            bool isBool = false;

            switch (method.MethodType)
            {
                case MethodType.StartsWith:
                case MethodType.EndsWith:
                case MethodType.SubStringOf:
                case MethodType.IsOf:
                    isBool = true;
                    break;

                case MethodType.Cast:
                    if ((string)((LiteralExpression)arguments[1]).Value == "Edm.Boolean")
                        isBool = true;
                    break;
            }

            return new MethodCallExpression(
                isBool ? MethodCallType.Boolean : MethodCallType.Normal,
                method,
                arguments
            );
        }

        private Expression[] ParseMethodCallArgumentList(Method method)
        {
            var arguments = new List<Expression>();

            while (true)
            {
                ExpectAny();

                arguments.Add(ParseCommon());

                ExpectAny();

                if (Current == SyntaxToken.Comma)
                {
                    MoveNext();
                }
                else if (Current == SyntaxToken.ParenClose)
                {
                    MoveNext();

                    break;
                }
                else
                {
                    throw new ODataException(ErrorMessages.Parser_CannotParseArgumentList);
                }
            }

            if (arguments.Count < method.ArgumentCount || arguments.Count > method.MaxArgumentCount)
            {
                if (method.ArgumentCount == method.MaxArgumentCount)
                {
                    throw new ODataException(String.Format(
                        ErrorMessages.Parser_IllegalArgumentCount,
                        method.MethodType,
                        method.ArgumentCount
                        ));
                }
                else
                {
                    throw new ODataException(String.Format(
                        ErrorMessages.Parser_IllegalVarArgumentCount,
                        method.MethodType,
                        method.ArgumentCount,
                        method.MaxArgumentCount
                        ));
                }
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                if (method.ArgumentTypes[i] == ArgumentType.StringLiteral)
                {
                    var literal = arguments[i] as LiteralExpression;

                    if (literal == null || !(literal.Value is string))
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Parser_ArgumentMustBeStringLiteral,
                            method.MethodType,
                            i + 1
                        ));
                    }
                }
            }

            return arguments.ToArray();
        }
    }
}
