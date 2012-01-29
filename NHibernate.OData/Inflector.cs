using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernate.OData
{
    /// <summary>
    /// Contains utility methods for working with English natural language strings.
    /// </summary>
    internal static class Inflector
    {
        private static readonly List<KeyValuePair<Regex, string>> _pluralRules = new List<KeyValuePair<Regex, string>>();
        private static readonly List<KeyValuePair<Regex, string>> _singularRules = new List<KeyValuePair<Regex, string>>();
        private static readonly List<string> _uncountables = new List<string>();

        static Inflector()
        {
            _uncountables.Add("equipment");
            _uncountables.Add("information");
            _uncountables.Add("rice");
            _uncountables.Add("money");
            _uncountables.Add("species");
            _uncountables.Add("series");
            _uncountables.Add("fish");
            _uncountables.Add("sheep");

            AddPlural("$", "s", true);
            AddPlural("s$", "s");
            AddPlural("(ax|test)is$", "$1es");
            AddPlural("(octop|vir)us$", "$1i");
            AddPlural("(alias|status)$", "$1es");
            AddPlural("(bu)s$", "$1ses");
            AddPlural("(buffal|tomat)o$", "$1oes");
            AddPlural("([ti])um$", "$1a");
            AddPlural("sis$", "ses");
            AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPlural("(hive)$", "$1s");
            AddPlural("([^aeiouy]|qu)y$", "$1ies");
            AddPlural("(x|ch|ss|sh)$", "$1es");
            AddPlural("(matr|vert|ind)(?:ix|ex)$", "$1ices");
            AddPlural("([m|l])ouse$", "$1ice");
            AddPlural("^(ox)$", "$1en");
            AddPlural("(quiz)$", "$1zes");

            AddSingular("s$", "");
            AddSingular("(n)ews$", "$1ews");
            AddSingular("([ti])a$", "$1um");
            AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingular("(^analy)ses$", "$1sis");
            AddSingular("([^f])ves$", "$1fe");
            AddSingular("(hive)s$", "$1");
            AddSingular("(tive)s$", "$1");
            AddSingular("([lr])ves$", "$1f");
            AddSingular("([^aeiouy]|qu)ies$", "$1y");
            AddSingular("(s)eries$", "$1eries");
            AddSingular("(m)ovies$", "$1ovie");
            AddSingular("(x|ch|ss|sh)es$", "$1");
            AddSingular("([m|l])ice$", "$1ouse");
            AddSingular("(bus)es$", "$1");
            AddSingular("(o)es$", "$1");
            AddSingular("(shoe)s$", "$1");
            AddSingular("(cris|ax|test)es$", "$1is");
            AddSingular("(octop|vir)i$", "$1us");
            AddSingular("(alias|status)es$", "$1");
            AddSingular("^(ox)en", "$1");
            AddSingular("(vert|ind)ices$", "$1ex");
            AddSingular("(matr)ices$", "$1ix");
            AddSingular("(quiz)zes$", "$1");

            AddIrregular("person", "people");
            AddIrregular("man", "men");
            AddIrregular("child", "children");
            AddIrregular("sex", "sexes");
            AddIrregular("move", "moves");
            AddIrregular("cow", "kine");
        }

        private static void AddIrregular(string singular, string plural)
        {
            AddPlural(singular.Substring(0, 1).ToLower() + singular.Substring(1) + "$", plural.Substring(0, 1).ToLower() + plural.Substring(1));
            AddPlural(singular.Substring(0, 1).ToUpper() + singular.Substring(1) + "$", plural.Substring(0, 1).ToUpper() + plural.Substring(1));
            AddSingular(plural.Substring(0, 1).ToLower() + plural.Substring(1) + "$", singular.Substring(0, 1).ToLower() + singular.Substring(1));
            AddSingular(plural.Substring(0, 1).ToUpper() + plural.Substring(1) + "$", singular.Substring(0, 1).ToUpper() + singular.Substring(1));
        }

        private static void AddPlural(string expression, string replacement)
        {
            AddPlural(expression, replacement, false);
        }

        private static void AddPlural(string expression, string replacement, bool caseSensitive)
        {
            var re = caseSensitive ? new Regex(expression) : new Regex(expression, RegexOptions.IgnoreCase);

            _pluralRules.Insert(0, new KeyValuePair<Regex, string>(re, replacement));
        }

        private static void AddSingular(string expression, string replacement)
        {
            AddSingular(expression, replacement, false);
        }

        private static void AddSingular(string expression, string replacement, bool caseSensitive)
        {
            var re = caseSensitive ? new Regex(expression) : new Regex(expression, RegexOptions.IgnoreCase);

            _singularRules.Insert(0, new KeyValuePair<Regex, string>(re, replacement));
        }

        /// <summary>
        /// Get the plural form of a singular text.
        /// </summary>
        /// <param name="value">The singular text for which to get
        /// the plural form.</param>
        /// <returns>The plural form of the singular text.</returns>
        public static string Pluralize(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (_uncountables.Contains(value))
                return value;

            foreach (var rule in _pluralRules)
            {
                if (rule.Key.Match(value).Success)
                {
                    return rule.Key.Replace(value, rule.Value);
                }
            }

            return value;
        }

        /// <summary>
        /// Get the singular for for a plural text.
        /// </summary>
        /// <param name="value">The plural text for which to
        /// get the singular form.</param>
        /// <returns>The singular form for the plural text.</returns>
        public static string Singularize(string value)
        {
            if (_uncountables.Contains(value))
                return value;

            foreach (var rule in _singularRules)
            {
                if (rule.Key.Match(value).Success)
                {
                    return rule.Key.Replace(value, rule.Value);
                }
            }

            return value;
        }

        /// <summary>
        /// Converts an underscore style text into a camelized form.
        /// </summary>
        /// <param name="value">The underscore style text to get the
        /// camelized form for.</param>
        /// <returns>The camelized form for the provided text.</returns>
        public static string Camelize(string value)
        {
            return Camelize(value, true);
        }

        /// <summary>
        /// Converts an underscore style text into a camelized form.
        /// </summary>
        /// <param name="value">The underscore style text to get the
        /// camelized form for.</param>
        /// <param name="firstLetterUppercase">true when the first
        /// letter of the returned value should be upper case. The default
        /// value for this parameter is true.</param>
        /// <returns>The camelized form for the provided text.</returns>
        public static string Camelize(string value, bool firstLetterUppercase)
        {
            if (firstLetterUppercase)
            {
                return
                    Regex.Replace(
                        Regex.Replace(value, "/(.?)", p => "::" + p.Groups[1].Value.ToUpperInvariant()),
                        "(?:^|_)(.)", p => p.Groups[1].Value.ToUpperInvariant()
                    );
            }
            else
            {
                return
                    value.Substring(0, 1).ToLowerInvariant() +
                    Camelize(value.Substring(1));
            }
        }

        /// <summary>
        /// Converts a camelized style text into an underscored form.
        /// </summary>
        /// <param name="value">The camelized style text to get the
        /// underscored form for.</param>
        /// <returns>The underscored form for the provided text.</returns>
        public static string Underscore(string value)
        {
            value = value.Replace("::", "/");
            value = Regex.Replace(value, "([A-Z]+)([A-Z][a-z])", p => p.Groups[1].Value + "_" + p.Groups[2].Value);
            value = Regex.Replace(value, "([a-z\\d])([A-Z])", p => p.Groups[1].Value + "_" + p.Groups[2].Value);
            value = value.Replace("-", "_");

            return value.ToLowerInvariant();
        }
    }
}
