/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.Extensions
 * FILE:        SemanticRefiner.cs
 * PURPOSE:     Some refinements neeeded after the parser is though.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text.RegularExpressions;
using ExtendedSystemObjects;

namespace Interpreter.Extensions
{
    internal static class SemanticRefiner
    {
        /// <summary>
        /// Refines the semantic structure.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CategorizedDictionary with improved if, else branches</returns>
        public static CategorizedDictionary<int, string> RefineSemanticStructure(this CategorizedDictionary<int, string> input)
        {
            var output = new CategorizedDictionary<int, string>();
            var nextKey = 0;

            foreach (var (key, category, value) in input)
            {
                switch (category.ToLowerInvariant())
                {
                    case "if":
                        var condition = ExtractCondition(value);
                        var ifBranch = ExtractBody(value);
                        output.Add("If_Condition", nextKey++,  condition);
                        output.Add("If_Branch", nextKey++,ifBranch);
                        break;

                    case "else":
                        var elseBranch = ExtractBody(value);
                        output.Add("Else_Branch", nextKey++,elseBranch);
                        break;

                    default:
                        output.Add(category, nextKey++,  value);
                        break;
                }
            }

            return output;
        }

        /// <summary>
        /// Removes the control statements.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CategorizedDictionary with removed control statements.</returns>
        public static CategorizedDictionary<int, string> RemoveControlStatements(this CategorizedDictionary<int, string> input)
        {
            var output = new CategorizedDictionary<int, string>();
            int nextKey = 0;

            foreach (var (key, category, value) in input)
            {
                var trimmed = value.Trim();

                if (trimmed.EndsWith(";"))
                {
                    trimmed = trimmed.Substring(0, trimmed.Length - 1).TrimEnd();
                }

                output.Add(category, nextKey++, trimmed);
            }

            return output;
        }

        /// <summary>
        /// Normalizes the jump targets.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CategorizedDictionary with improved jump targets.</returns>
        public static CategorizedDictionary<int, string> NormalizeJumpTargets(this CategorizedDictionary<int, string> input)
        {
            var output = new CategorizedDictionary<int, string>();
            int nextKey = 0;

            foreach (var (key, category, value) in input)
            {
                string normalized = value.Trim();

                switch (category.ToLowerInvariant())
                {
                    case "label":
                        // Label(x) → x
                        normalized = ExtractInnerValue(value, "Label");
                        output.Add("Label", nextKey++, normalized);
                        break;

                    case "goto":
                        // goto(x) → x
                        normalized = ExtractInnerValue(value, "goto");
                        output.Add("Goto", nextKey++, normalized);
                        break;

                    default:
                        output.Add(category, nextKey++, value);
                        break;
                }
            }

            return output;
        }

        private static string ExtractInnerValue(string value, string keyword)
        {
            var pattern = $@"\b{keyword}\s*\(\s*(.*?)\s*\)";
            var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : value.Trim();
        }

        private static string ExtractCondition(string value)
        {
            var m = Regex.Match(value, @"if\s*\((.*?)\)");
            return m.Success ? m.Groups[1].Value.Trim() : string.Empty;
        }

        private static string ExtractBody(string value)
        {
            var m = Regex.Match(value, @"\{(.*?)\}", RegexOptions.Singleline);
            return m.Success ? m.Groups[1].Value.Trim() : string.Empty;
        }
    }
}
