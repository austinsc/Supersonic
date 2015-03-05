// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Inflector.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Supersonic
{
    /// <summary>
    /// Summary for the Inflector class
    /// </summary>
    public static class Inflector
    {
        private static readonly PluralizationService _pluralizationService = PluralizationService.CreateService(CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns the plural form of the specified word.
        /// </summary>
        /// <param name="word">The word to be made plural.</param>
        /// <returns>A System.String that is the plural form of the input parameter.</returns>
        public static string MakePlural(string word)
        {
            return _pluralizationService.Pluralize(word);
        }

        /// <summary>
        /// Returns the singular form of the specified word.
        /// </summary>
        /// <param name="word">The the word to be made singular.</param>
        /// <returns>The singular form of the input parameter.</returns>
        public static string MakeSingular(string word)
        {
            return _pluralizationService.Singularize(word);
        }

        /// <summary>
        /// Determines if the specified word is plural.
        /// </summary>
        /// <param name="word">The System.String value to be analyzed.</param>
        /// <returns>true if the word is plural; otherwise, false.</returns>
        public static bool IsPlural(string word)
        {
            return _pluralizationService.IsPlural(word);
        }

        /// <summary>
        /// Determines if the specified word is singular.
        /// </summary>
        /// <param name="word">The System.String value to be analyzed.</param>
        /// <returns>true if the word is singular; otherwise, false.</returns>
        /// <returns></returns>
        public static bool IsSingular(string word)
        {
            return _pluralizationService.IsSingular(word);
        }

        /// <summary>
        /// Converts the string to title case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string ToTitleCase(string word)
        {
            var s = Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])", match => match.Captures[0].Value.ToUpper());
            var digit = false;
            var a = string.Empty;
            foreach (var c in s)
                if (Char.IsDigit(c))
                {
                    digit = true;
                    a = a + c;
                }
                else
                {
                    if (digit && Char.IsLower(c))
                        a = a + Char.ToUpper(c);
                    else
                        a = a + c;
                    digit = false;
                }
            return a;
        }

        /// <summary>
        /// Converts the string to human case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        /// <returns></returns>
        public static string ToHumanCase(string lowercaseAndUnderscoredWord)
        {
            return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }

        /// <summary>
        /// Adds the underscores.
        /// </summary>
        /// <param name="pascalCasedWord">The pascal cased word.</param>
        /// <returns></returns>
        public static string AddUnderscores(string pascalCasedWord)
        {
            return
                Regex.Replace(Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])", "$1_$2"), @"[-\s]", "_").ToLower();
        }

        /// <summary>
        /// Makes the initial caps.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakeInitialCaps(string word)
        {
            return String.Concat(word.Substring(0, 1).ToUpper(), word.Substring(1).ToLower());
        }

        /// <summary>
        /// Makes the initial lower case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakeInitialLowerCase(string word)
        {
            return String.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
        }

        /// <summary>
        /// Determine whether the passed string is numeric, by attempting to parse it to a double
        /// </summary>
        /// <param name="str">The string to evaluated for numeric conversion</param>
        /// <returns>
        /// <c>true</c> if the string can be converted to a number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStringNumeric(string str)
        {
            double result;
            return (double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result));
        }

        /// <summary>
        /// Adds the ordinal suffix.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public static string AddOrdinalSuffix(string number)
        {
            if (IsStringNumeric(number))
            {
                var n = int.Parse(number);
                var nMod100 = n % 100;

                if (nMod100 >= 11 && nMod100 <= 13)
                    return String.Concat(number, "th");

                switch (n % 10)
                {
                    case 1:
                        return String.Concat(number, "st");
                    case 2:
                        return String.Concat(number, "nd");
                    case 3:
                        return String.Concat(number, "rd");
                    default:
                        return String.Concat(number, "th");
                }
            }
            return number;
        }

        /// <summary>
        /// Converts the underscores to dashes.
        /// </summary>
        /// <param name="underscoredWord">The underscored word.</param>
        /// <returns></returns>
        public static string ConvertUnderscoresToDashes(string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }
    }
}