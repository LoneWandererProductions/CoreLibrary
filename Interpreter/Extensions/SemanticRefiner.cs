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

            int i = 0;
            while (i < input.Count)
            {
                var entry = input.GetCategoryAndValue(i);
                if (entry == null)
                {
                    i++;
                    continue;
                }

                var (cat, val) = entry.Value;

                if (cat == "If_Condition")
                {
                    // Add the condition
                    output.Add("If_Condition", nextKey++, val);

                    // Add open
                    output.Add("If_Open", nextKey++, null);

                    i++; // Move to next item

                    // Copy all following commands until an 'Else_' or 'If_End' or another control token
                    while (i < input.Count &&
                           input.TryGetCategory(i, out var nextCat) &&
                           nextCat != "Else_Condition" &&
                           nextCat != "Else_Open" &&
                           nextCat != "If_End" &&
                           nextCat != "Else_End" &&
                           nextCat != "If_Condition")
                    {
                        var subEntry = input.GetCategoryAndValue(i);
                        output.Add(subEntry?.Category ?? "Command", nextKey++, subEntry?.Value);
                        i++;
                    }

                    // Add end
                    output.Add("If_End", nextKey++, null);
                }
                else
                {
                    // Everything else just gets forwarded
                    output.Add(cat, nextKey++, val);
                    i++;
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
                if (string.IsNullOrEmpty(value)) continue;

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
