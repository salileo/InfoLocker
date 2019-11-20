using System;
using System.Text.RegularExpressions;

namespace HgCo.WindowsLive.SkyDrive.Support
{
    /// <summary>
    /// Provides methods for handling Regex synchronized (thread safe).
    /// </summary>
    internal static class RegexHelper
    {
        /// <summary>
        /// Indicates whether the regular expression finds a match in the input string.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <returns><c>true</c> if the regular expression finds a match; otherwise, <c>false</c>.</returns>
        public static bool IsMatch(Regex regex, string input)
        {
            bool isMatch = false;
            lock (regex)
            {
                isMatch = regex.IsMatch(input);
            }
            return isMatch;
        }

        /// <summary>
        /// Searches an input string for an occurrence of a regular expression and returns the precise result as a single Match object.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <returns>A regular expression Match object.</returns>
        public static Match Match(Regex regex, string input)
        {
            Match match = null;
            lock (regex)
            {
                match = regex.Match(input);
            }
            return match;
        }

        /// <summary>
        /// Searches an input string for all occurrences of a regular expression and returns all the successful matches as if Match were called numerous times.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <returns>A MatchCollection of the Match objects found by the search.</returns>
        public static MatchCollection Matches(Regex regex, string input)
        {
            MatchCollection matches = null;
            lock (regex)
            {
                matches = regex.Matches(input);
            }
            return matches;
        }

        /// <summary>
        /// Within a specified input string, replaces strings that match a regular expression pattern with a specified replacement string. 
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="evaluator">A custom method that examines each match and returns either the original matched string or a replacement string.</param>
        /// <returns>A new string that is identical to the input string, except that a replacement string takes the place of each matched string.</returns>
        public static string Replace(Regex regex, string input, MatchEvaluator evaluator)
        {
            string inputReplaced = null;
            lock (regex)
            {
                inputReplaced = regex.Replace(input, evaluator);
            }
            return inputReplaced;
        }
    }
}
