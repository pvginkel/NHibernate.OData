using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernate.OData
{
    internal class Lexer
    {
        private static readonly CultureInfo ParseCulture = CultureInfo.InvariantCulture;
        private static readonly Regex DateTimeRegex = new Regex("^(\\d{4})-(\\d{1,2})-(\\d{1,2})T(\\d{1,22}):(\\d{2})(?::(\\d{2})(?:\\.(\\d{7}))?)?$");
        private static readonly Regex GuidRegex = new Regex("^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$");
        private static readonly Regex DurationRegex = new Regex("^(-)?P(?:(\\d+)Y)?(?:(\\d+)M)?(?:(\\d+)D)?T?(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+(?:\\.\\d*)?)S)?$");
        private readonly string _source;
        private int _offset;
        private int _current;

        public Lexer(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _source = source;
        }

        public IList<Token> ToList()
        {
            _offset = 0;
            _current = 0;

            var result = new List<Token>();
            Token token;

            while ((token = GetNext()) != null)
            {
                result.Add(token);
            }

            return result;
        }

        private Token GetNext()
        {
            if (_offset >= _source.Length)
                return null;

            while (_offset < _source.Length && Char.IsWhiteSpace(_source[_offset]))
            {
                _offset++;
            }

            if (_offset >= _source.Length)
                return null;

            _current = _offset;

            char c = _source[_current];

            switch (c)
            {
                case '-':
                    return ParseSign();

                case '\'':
                    return ParseString();

                case '(':
                case ')':
                case ',':
                case '/':
                    return ParseSyntax();

                default:
                    if (Char.IsNumber(c))
                    {
                        return ParseNumeric();
                    }
                    else if (IsIdentifierStartChar(c))
                    {
                        return ParseIdentifier(false);
                    }
                    else
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Lexer_UnexpectedCharacter, c, _current
                        ));
                    }
            }
        }

        private bool IsIdentifierStartChar(char c)
        {
            // Definition for names taken from
            // http://msdn.microsoft.com/en-us/library/aa664670.aspx.
            // Only exception here is that we also include the '$'
            // sign. This is for working with '$value' which has a
            // special meaning.

            return c == '_' || c == '$' || Char.IsLetter(c);
        }

        private bool IsIdentifierChar(char c)
        {
            return IsIdentifierStartChar(c) || Char.IsDigit(c);
        }

        private Token ParseSign()
        {
            _current++;

            if (Char.IsDigit(_source[_current]))
                return ParseNumeric();
            else
                return ParseIdentifier(true);
        }

        private LiteralToken ParseString()
        {
            var sb = new StringBuilder();
            bool hadEnd = false;

            for (_current++; _current < _source.Length; _current++)
            {
                char c = _source[_current];

                if (c == '\'')
                {
                    // Two consecutive quotes translate to a single quote in
                    // the string. This is not in the spec (2.2.2), but seems
                    // the logical thing to do (and at StackOverflow on
                    // http://stackoverflow.com/questions/3979367 they seem
                    // to think the same thing).

                    if (
                        _current < _source.Length - 1 &&
                        _source[_current + 1] == '\''
                    ) {
                        _current++;
                        sb.Append('\'');
                    }
                    else
                    {
                        hadEnd = true;

                        break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (!hadEnd)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Lexer_UnterminatedString, _offset
                ));
            }

            _offset = _current + 1;

            return new LiteralToken(sb.ToString());
        }

        private Token ParseNumeric()
        {
            bool floating = false;
            char c;

            for (_current++; _current < _source.Length; _current++)
            {
                c = _source[_current];

                if (c == '.')
                {
                    if (floating)
                        break;

                    floating = true;
                }
                else if (!Char.IsDigit(c))
                {
                    break;
                }
            }

            bool haveExponent = false;

            if (_current < _source.Length)
            {
                c = _source[_current];

                if (c == 'E' || c == 'e')
                {
                    _current++;

                    if (_source[_current] == '-')
                        _current++;

                    int? exponentEnd = _current == _source.Length ? null : SkipDigits(_current);

                    if (!exponentEnd.HasValue)
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Lexer_ExpectedDigitsAfterExponent, _offset
                        ));
                    }

                    _current = exponentEnd.Value;

                    haveExponent = true;

                    if (_current < _source.Length)
                    {
                        c = _source[_current];

                        if (c == 'm' || c == 'M')
                        {
                            throw new ODataException(String.Format(
                                ErrorMessages.Lexer_DecimalCannotHaveExponent, _offset
                            ));
                        }
                        else if (c == 'l' || c == 'L')
                        {
                            throw new ODataException(String.Format(
                                ErrorMessages.Lexer_LongCannotHaveExponent, _offset
                            ));
                        }
                    }
                }
            }

            string text = _source.Substring(_offset, _current - _offset);
            object value;

            if (_current < _source.Length)
            {
                c = _source[_current];

                switch (c)
                {
                    case 'F':
                    case 'f':
                        value = float.Parse(text, ParseCulture);
                        _current++;
                        break;

                    case 'D':
                    case 'd':
                        value = double.Parse(text, ParseCulture);
                        _current++;
                        break;

                    case 'M':
                    case 'm':
                        value = decimal.Parse(text, ParseCulture);
                        _current++;
                        break;

                    case 'L':
                    case 'l':
                        value = long.Parse(text, ParseCulture);
                        _current++;
                        break;

                    default:
                        if (floating || haveExponent)
                            value = double.Parse(text, ParseCulture);
                        else
                            value = int.Parse(text, ParseCulture);
                        break;
                }
            }
            else
            {
                if (floating || haveExponent)
                    value = double.Parse(text, ParseCulture);
                else
                    value = int.Parse(text, ParseCulture);
            }

            _offset = _current;

            return new LiteralToken(value);
        }

        private int? SkipDigits(int current)
        {
            if (!Char.IsDigit(_source[current]))
                return null;

            current++;

            while (current < _source.Length && Char.IsDigit(_source[current]))
            {
                current++;
            }

            return current;
        }

        private Token ParseSyntax()
        {
            SyntaxToken token;

            switch (_source[_current])
            {
                case '(': token = SyntaxToken.ParenOpen; break;
                case ')': token = SyntaxToken.ParenClose; break;
                case '/': token = SyntaxToken.Slash; break;
                case ',': token = SyntaxToken.Comma; break;
                default: throw new InvalidOperationException("Unknown token");
            }

            _offset = _current + 1;

            return token;
        }

        private Token ParseIdentifier(bool minus)
        {
            for (_current++; _current < _source.Length; _current++)
            {
                char c = _source[_current];

                if (!IsIdentifierChar(c))
                    break;
            }

            string name = _source.Substring(_offset, _current - _offset);

            int lastOffset = _offset;
            _offset = _current;

            switch (name)
            {
                case "INF":
                    return LiteralToken.PositiveInfinity;

                case "-INF":
                    return LiteralToken.NegativeInfinity;

                case "Nan":
                    return LiteralToken.NaN;

                case "true":
                    return LiteralToken.True;

                case "false":
                    return LiteralToken.False;

                case "null":
                    return LiteralToken.Null;

                case "-":
                    return SyntaxToken.Negative;

                default:
                    if (minus)
                    {
                        // Reset the offset.

                        _offset = lastOffset + 1;

                        return SyntaxToken.Negative;
                    }
                    break;
            }

            if (_offset < _source.Length && _source[_offset] == '\'')
            {
                StringType stringType;

                switch (name)
                {
                    case "X": stringType = StringType.Binary; break;
                    case "binary": stringType = StringType.Binary; break;
                    case "datetime": stringType = StringType.DateTime; break;
                    case "guid": stringType = StringType.Guid; break;
                    case "time": stringType = StringType.Time; break;
                    case "datetimeoffset": stringType = StringType.DateTimeOffset; break;
                    default: stringType = StringType.None; break;
                }

                if (stringType != StringType.None && _source[_offset] == '\'')
                {
                    var content = ParseString();

                    return ParseSpecialString((string)content.Value, stringType);
                }
            }

            return new IdentifierToken(name);
        }

        private Token ParseSpecialString(string value, StringType stringType)
        {
            switch (stringType)
            {
                case StringType.Binary:
                    return ParseBinaryString(value);

                case StringType.DateTime:
                    return ParseDateTimeString(value);

                case StringType.DateTimeOffset:
                    return ParseDateTimeOffsetString(value);

                case StringType.Guid:
                    return ParseGuidString(value);

                case StringType.Time:
                    return ParseTimeString(value);

                default:
                    throw new ArgumentOutOfRangeException("stringType");
            }
        }

        private Token ParseBinaryString(string value)
        {
            if (value.Length % 2 == 0)
            {
                byte[] result = new byte[value.Length / 2];

                for (int i = 0; i < result.Length; i++)
                {
                    if (HttpUtil.IsHex(value[i * 2]) && HttpUtil.IsHex(value[i * 2 + 1]))
                    {
                        result[i] = (byte)(HttpUtil.HexToInt(value[i * 2]) * 16 + HttpUtil.HexToInt(value[i * 2 + 1]));
                    }
                    else
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Lexer_InvalidBinaryFormat, _offset
                        ));
                    }
                }

                return new LiteralToken(result);
            }
            else
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Lexer_InvalidBinaryFormat, _offset
                ));
            }
        }

        private Token ParseDateTimeString(string value)
        {
            // We parse this date/time because we want to take care of the optional
            // seconds and nanoseconds.

            var match = DateTimeRegex.Match(value);

            if (match.Success)
            {
                int year = int.Parse(match.Groups[1].Value, ParseCulture);
                int month = int.Parse(match.Groups[2].Value, ParseCulture);
                int day = int.Parse(match.Groups[3].Value, ParseCulture);
                int hour = int.Parse(match.Groups[4].Value, ParseCulture);
                int minute = int.Parse(match.Groups[5].Value, ParseCulture);
                int second = match.Groups[6].Value.Length > 0 ? int.Parse(match.Groups[6].Value, ParseCulture) : 0;
                int nanoSecond = match.Groups[7].Value.Length > 0 ? int.Parse(match.Groups[7].Value, ParseCulture) : 0;

                // We let DateTime take care of validating the input.

                return new LiteralToken(new DateTime(year, month, day, hour, minute, second, nanoSecond / 1000));
            }
            else
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Lexer_InvalidDateTimeFormat, _offset
                ));
            }
        }

        private Token ParseDateTimeOffsetString(string value)
        {
            // Let DateTime take care of validating the input. "o" should be the
            // XMLSchema date/time with timezone format.

            return new LiteralToken(DateTime.ParseExact(value, "o", ParseCulture));
        }

        private Token ParseGuidString(string value)
        {
            if (!GuidRegex.IsMatch(value))
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Lexer_InvalidGuidFormat, _offset
                ));
            }

            return new LiteralToken(new Guid(value));
        }

        private Token ParseTimeString(string value)
        {
            var match = DurationRegex.Match(value);

            if (match.Success)
            {
                bool negative = match.Groups[1].Value == "-";
                int year = match.Groups[2].Value.Length > 0 ? int.Parse(match.Groups[2].Value, ParseCulture) : 0;
                int month = match.Groups[3].Value.Length > 0 ? int.Parse(match.Groups[2].Value, ParseCulture) : 0;
                int day = match.Groups[4].Value.Length > 0 ? int.Parse(match.Groups[2].Value, ParseCulture) : 0;
                int hour = match.Groups[5].Value.Length > 0 ? int.Parse(match.Groups[2].Value, ParseCulture) : 0;
                int minute = match.Groups[6].Value.Length > 0 ? int.Parse(match.Groups[2].Value, ParseCulture) : 0;
                double second = match.Groups[7].Value.Length > 0 ? double.Parse(match.Groups[2].Value, ParseCulture) : 0;

                return new LiteralToken(new XmlTimeSpan(!negative, year, month, day, hour, minute, second));
            }
            else
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Lexer_InvalidDurationFormat, _offset
                ));
            }
        }

        private enum StringType
        {
            None,
            Binary,
            DateTime,
            Guid,
            Time,
            DateTimeOffset
        }
    }
}
