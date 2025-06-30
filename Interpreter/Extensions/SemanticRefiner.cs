using System.Text.RegularExpressions;
using ExtendedSystemObjects;

namespace Interpreter.Extensions
{
    internal static class SemanticRefiner
    {
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
