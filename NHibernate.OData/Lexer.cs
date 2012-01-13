using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class Lexer
    {
        private readonly string _source;
        private int _offset;
        private int _current;

        public Lexer(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _source = source;
        }

        public Token GetNext()
        {
            if (_offset >= _source.Length)
                return null;

            while (Char.IsWhiteSpace(_source[_offset]))
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
                    else if (IsNameStartChar(c))
                    {
                        return ParseName();
                    }
                    else
                    {
                        throw new ODataException(String.Format(
                            ErrorMessages.Tokenizer_UnexpectedCharacter, c, _current
                        ));
                    }
            }
        }

        private bool IsNameStartChar(char c)
        {
            // Definition for names taken from
            // http://msdn.microsoft.com/en-us/library/aa664670.aspx.
            // Only exception here is that we also include the '$'
            // sign. This is for working with '$value' which has a
            // special meaning.

            return c == '_' || c == '$' || Char.IsLetter(c);
        }

        private bool IsNameChar(char c)
        {
            return IsNameStartChar(c) || Char.IsDigit(c);
        }

        private Token ParseSign()
        {
            _current++;

            char c = _source[_current];

            if (Char.IsDigit(c))
                return ParseNumeric();
            else
                return SyntaxToken.Minus;
        }

        private Token ParseString()
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
                    ErrorMessages.Tokenizer_UnterminatedString, _offset
                ));
            }

            _offset = _current + 1;

            return new LiteralToken(sb.ToString());
        }

        private Token ParseNumeric()
        {
            if (Char.IsDigit(_source[_offset]))
            {
                var dateTimeToken = TryParseDateTime();

                if (dateTimeToken != null)
                    return dateTimeToken;
            }

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
                            ErrorMessages.Tokenizer_ExpectedDigitsAfterExponent, _offset
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
                                ErrorMessages.Tokenizer_DecimalCannotHaveExponent, _offset
                            ));
                        }
                        else if (c == 'l' || c == 'L')
                        {
                            throw new ODataException(String.Format(
                                ErrorMessages.Tokenizer_LongCannotHaveExponent, _offset
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
                        value = float.Parse(text);
                        break;

                    case 'D':
                    case 'd':
                        value = double.Parse(text);
                        break;

                    case 'M':
                    case 'm':
                        value = decimal.Parse(text);
                        break;

                    case 'L':
                    case 'l':
                        value = long.Parse(text);
                        break;

                    default:
                        if (floating || haveExponent)
                            value = double.Parse(text);
                        else
                            value = int.Parse(text);
                        break;
                }
            }
            else
            {
                if (floating || haveExponent)
                    value = double.Parse(text);
                else
                    value = int.Parse(text);
            }

            return new LiteralToken(value);
        }

        private LiteralToken TryParseDateTime()
        {
            // Date/times are parsed following 2.2.2. Note that we do not throw
            // when we don't match a date/time. We just interpret the value
            // as a number and let some other part of the tokenizer throw.
            // This includes e.g. 13 as the month value.

            int yearStart = _current;
            int? yearEnd = SkipDigits(yearStart);

            if (
                !yearEnd.HasValue ||
                yearEnd.Value - yearStart != 4 ||
                _source[yearEnd.Value + 1] != '-'
            )
                return null;

            int monthStart = yearEnd.Value + 1;
            int? monthEnd = SkipDigits(monthStart);

            if (
                !monthEnd.HasValue ||
                monthEnd.Value - monthStart > 2 ||
                _source[monthEnd.Value + 1] != '-'
            )
                return null;

            int dayStart = monthEnd.Value + 1;
            int? dayEnd = SkipDigits(dayStart);

            if (
                !dayEnd.HasValue ||
                dayEnd.Value - dayStart > 2 ||
                _source[dayEnd.Value + 1] != 'T'
            )
                return null;

            int hourStart = dayEnd.Value + 1;
            int? hourEnd = SkipDigits(hourStart);

            if (
                !hourEnd.HasValue ||
                hourEnd.Value - hourStart > 2 ||
                _source[hourEnd.Value + 1] != ':'
            )
                return null;

            int minuteStart = hourEnd.Value + 1;
            int? minuteEnd = SkipDigits(minuteStart);

            if (
                !minuteEnd.HasValue ||
                minuteEnd.Value - minuteStart > 2
            )
                return null;

            int offset = minuteEnd.Value;

            int? secondStart = null;
            int? secondEnd = null;
            int? nanoSecondStart = null;
            int? nanoSecondEnd = null;

            if (_source[minuteEnd.Value + 1] == ':')
            {
                secondStart = minuteEnd.Value + 1;
                secondEnd = SkipDigits(secondStart.Value);

                if (
                    !secondEnd.HasValue ||
                    secondEnd.Value - secondStart.Value > 2
                )
                    return null;

                offset = secondEnd.Value;

                if (_source[secondEnd.Value + 1] == '.')
                {
                    nanoSecondStart = secondEnd.Value + 1;
                    nanoSecondEnd = SkipDigits(nanoSecondStart.Value);

                    if (
                        !nanoSecondEnd.HasValue ||
                        nanoSecondEnd.Value - nanoSecondStart.Value > 7
                    )
                        return null;

                    offset = nanoSecondEnd.Value;
                }
            }

            int year = int.Parse(_source.Substring(yearStart, yearEnd.Value - yearStart));
            int month = int.Parse(_source.Substring(monthStart, monthEnd.Value - monthStart));
            int day = int.Parse(_source.Substring(dayStart, dayEnd.Value - dayStart));
            int hour = int.Parse(_source.Substring(hourStart, hourEnd.Value - hourStart));
            int minute = int.Parse(_source.Substring(minuteStart, minuteEnd.Value - minuteStart));

            int second = 0;
            int nanoSecond = 0;

            if (secondStart.HasValue)
                second = int.Parse(_source.Substring(secondStart.Value, secondEnd.Value - secondStart.Value));
            if (nanoSecondStart.HasValue)
                nanoSecond = int.Parse(_source.Substring(nanoSecondStart.Value, nanoSecondEnd.Value - nanoSecondStart.Value));

            if (month == 0 || month > 12)
                return null;
            if (day == 0 || day > 31)
                return null;
            if (minute > 59)
                return null;
            if (second > 59)
                return null;

            _offset = offset;

            // Nanoseconds are truncated because DateTime expects milliseconds.

            return new LiteralToken(new DateTime(year, month, day, hour, minute, second, nanoSecond / 1000));
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
                case '-': token = SyntaxToken.Minus; break;
                case '(': token = SyntaxToken.ParenOpen; break;
                case ')': token = SyntaxToken.ParenClose; break;
                case '/': token = SyntaxToken.Slash; break;
                case ',': token = SyntaxToken.Comma; break;
                default: throw new InvalidOperationException("Unknown token");
            }

            _offset = _current + 1;

            return token;
        }

        private Token ParseName()
        {
            for (_current++; _current < _source.Length; _current++)
            {
                char c = _source[_current];

                if (IsNameChar(c))
                    break;
            }

            string name = _source.Substring(_offset, _current - _offset);

            _offset = _current;

            return new NameToken(name);
        }
    }
}
