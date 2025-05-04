using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter
{
    /// <summary>
    ///     Basic if else Parser
    /// </summary>
    internal static class IrtParserIfElse
    {
        private static int _idCounter;

        /// <summary>
        ///     Generates if else commands.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>Our if clause ready to be added to Command Register</returns>
        internal static List<(string Category, string Value)> GenerateIfElseCommands(string block)
        {
            var ifElse = ParseIfElseClauses(block);
            var categorizeIfElse = CategorizeIfElseClauses(ifElse);
            return GenerateFormattedOutput(categorizeIfElse);
        }

        /// <summary>
        /// Generates the formatted output.
        /// </summary>
        /// <param name="categorizedClauses">The categorized clauses.</param>
        /// <returns></returns>
        internal static List<(string Category, string Value)> GenerateFormattedOutput(
            List<(string Category, string Clause, string ParentCategory)> categorizedClauses)
        {
            var output = new List<(string Category, string Value)>();
            var openBlocks = new Stack<string>();

            void ProcessClause((string Category, string Clause, string ParentCategory) clause)
            {
                var categoryParts = clause.Category?.Split('_');
                string layer;

                switch (categoryParts?.Length)
                {
                    case > 2:
                        layer = categoryParts[2];
                        break;
                    default:
                        layer = "unknown";
                        break;
                }

                // Process the 'if' block
                if (clause.Category.StartsWith("if"))
                {
                    output.Add(($"IF_LAYER_{layer}_BEGIN", clause.Clause));
                    output.Add(("COMMAND", "// Content of If block goes here"));
                    openBlocks.Push(clause.Category);

                    // Process nested clauses
                    foreach (var nestedClause in categorizedClauses.Where(c => c.ParentCategory == clause.Category)
                                 .ToList())
                    {
                        ProcessClause(nestedClause);
                    }

                    output.Add(($"IF_LAYER_{layer}_END", ""));
                    openBlocks.Pop();
                }
                // Process the 'else' block
                else if (clause.Category.StartsWith("else"))
                {
                    output.Add(($"ELSE_LAYER_{layer}_BEGIN", clause.Clause));
                    output.Add(("COMMAND", "// Content of Else block goes here"));
                    openBlocks.Push(clause.Category);

                    // Process nested clauses
                    foreach (var nestedClause in categorizedClauses.Where(c => c.ParentCategory == clause.Category)
                                 .ToList())
                    {
                        ProcessClause(nestedClause);
                    }

                    output.Add(($"ELSE_LAYER_{layer}_END", ""));
                    openBlocks.Pop();
                }
            }

            // Process only root elements (those without a parent)
            foreach (var rootClause in categorizedClauses.Where(c => string.IsNullOrEmpty(c.ParentCategory)).ToList())
            {
                ProcessClause(rootClause);
            }

            return output;
        }

        /// <summary>
        ///     Categorizes if else clauses.
        /// </summary>
        /// <param name="clauses">The clauses.</param>
        /// <returns>Converted List of parameters we will use to create a parameter list</returns>
        internal static List<(string Category, string Clause, string ParentCategory)> CategorizeIfElseClauses(
            List<IfElseClause> clauses)
        {
            var tupleList = new List<(string Category, string Clause, string ParentCategory)>();
            var idToCategory = new Dictionary<string, string>();

            // Process the outermost layer (Layer 0)
            foreach (var clause in clauses.Where(c => c.Layer == 0).ToList())
            {
                var categoryIf = $"if_layer_{clause.Id}";
                var categoryElse = $"else_layer_{clause.Id}";

                idToCategory[clause.Id] = categoryIf;

                tupleList.Add((categoryIf, clause.IfClause, null));
                tupleList.Add((categoryElse, clause.ElseClause, null));
            }

            // Process deeper layers (Layer > 0)
            foreach (var layer in clauses.Where(c => c.Layer > 0).OrderBy(c => c.Layer))
            {
                var parent = clauses.FirstOrDefault(c => c.Layer == layer.Layer - 1);
                var parentCategory = idToCategory.ContainsKey(parent.Id) ? idToCategory[parent.Id] : null;

                var categoryIf = $"if_layer_{layer.Id}";
                var categoryElse = $"else_layer_{layer.Id}";

                // Add the deeper layer clauses to the tuple list
                tupleList.Add((categoryIf, layer.IfClause, parentCategory));
                tupleList.Add((categoryElse, layer.ElseClause, parentCategory));

                // Update category mapping
                idToCategory[layer.Id] = categoryIf;
            }

            return tupleList;
        }

        /// <summary>
        ///     Parses the given code string to extract all If-Else clauses.
        /// </summary>
        /// <param name="code">The code string containing If-Else clauses.</param>
        /// <returns>A list of IfElseClause objects representing each If-Else clause found.</returns>
        internal static List<IfElseClause> ParseIfElseClauses(string code)
        {
            var clauses = new List<IfElseClause>();
            ParseIfElseClausesRecursively(code, clauses, 0);
            return clauses;
        }

        /// <summary>
        ///     Recursively parses If-Else clauses from the code string.
        /// </summary>
        /// <param name="code">The code string containing If-Else clauses.</param>
        /// <param name="clauses">The list to which found If-Else clauses are added.</param>
        /// <param name="layer">The current nesting layer of the If-Else clauses.</param>
        private static void ParseIfElseClausesRecursively(string code, ICollection<IfElseClause> clauses, int layer)
        {
            while (true)
            {
                var ifIndex = IrtKernel.FindFirstKeywordIndex(code, "if");
                if (ifIndex == -1)
                {
                    break;
                }

                var codeFromIfIndex = code.Substring(ifIndex);
                var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(codeFromIfIndex);

                if (string.IsNullOrWhiteSpace(block))
                {
                    break;
                }

                var ifElseClause = CreateIfElseClause(code, block, elsePosition, layer);
                clauses.Add(ifElseClause);

                // Ensure extracted clauses are valid before recursive calls
                var innerIfClause = ExtractInnerIfElse(ifElseClause.IfClause);
                if (!string.IsNullOrEmpty(innerIfClause))
                {
                    ParseIfElseClausesRecursively(innerIfClause, clauses, layer + 1);
                }

                var innerElseClause = ExtractInnerIfElse(ifElseClause.ElseClause);
                if (!string.IsNullOrEmpty(innerElseClause))
                {
                    ParseIfElseClausesRecursively(innerElseClause, clauses, layer + 1);
                }

                break;
            }
        }


        /// <summary>
        ///     Creates an IfElseClause object from the extracted If-Else block.
        /// </summary>
        /// <param name="code">The original code string.</param>
        /// <param name="block">The extracted block containing If-Else code.</param>
        /// <param name="elsePosition">The position of the 'else' keyword in the block.</param>
        /// <param name="layer">The nesting layer of the If-Else clause.</param>
        /// <returns>An IfElseClause object representing the parsed If-Else clause.</returns>
        private static IfElseClause CreateIfElseClause(string code, string block, int elsePosition, int layer)
        {
            string ifClause;
            string elseClause = null;

            if (elsePosition == -1)
            {
                // No else clause, the entire block is the ifClause
                ifClause = block.Trim();
            }
            else
            {
                // Split the block into ifClause and elseClause
                ifClause = block.Substring(0, elsePosition).Trim();
                elseClause = block.Substring(elsePosition).Trim();
            }

            // Generate a unique ID for each clause
            var uniqueId = GenerateUniqueId();

            return new IfElseClause
            {
                Id = uniqueId,
                Parent = code,
                IfClause = ifClause,
                ElseClause = elseClause,
                Layer = layer
            };
        }

        /// <summary>
        ///     Removes the outermost If-Else structure from the given code string.
        /// </summary>
        /// <param name="code">The code string containing an If-Else structure.</param>
        /// <returns>The code string with the outer If-Else structure removed.</returns>
        private static string ExtractInnerIfElse(string code)
        {
            // Check if the code contains an "if" keyword with an opening parenthesis
            if (!IrtKernel.ContainsKeywordWithOpenParenthesis(code, "if"))
            {
                return code;
            }

            // Find the first "if" keyword and the corresponding opening brace '{'
            var ifIndex = code.IndexOf("if", StringComparison.OrdinalIgnoreCase);
            var openBraceIndex = code.IndexOf('{', ifIndex);
            if (openBraceIndex == -1)
            {
                return code; // No opening brace found, return original code
            }

            // Find the matching closing brace for this opening brace
            var closeBraceIndex = FindBlockEnd(code, openBraceIndex);
            if (closeBraceIndex == -1)
            {
                return code; // No closing brace found, return original code
            }

            // Look for the next "if" keyword within this block
            var nextIfIndex = code.IndexOf("if", openBraceIndex + 1, closeBraceIndex - (openBraceIndex + 1),
                StringComparison.OrdinalIgnoreCase);

            if (nextIfIndex == -1)
                // No nested "if" found, return the entire block
            {
                return code.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1).Trim();
            }

            // Otherwise, return the code from the nested "if" onward
            return code.Substring(nextIfIndex, closeBraceIndex - nextIfIndex + 1).Trim();
        }

        /// <summary>
        ///     Finds the index of the closing brace that matches the opening brace starting at the given index.
        /// </summary>
        /// <param name="code">The code string.</param>
        /// <param name="start">The index of the opening brace.</param>
        /// <returns>The index of the matching closing brace, or -1 if not found.</returns>
        private static int FindBlockEnd(string code, int start)
        {
            var braceCount = 0;

            for (var i = start; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '{':
                        braceCount++;
                        break;
                    case '}':
                        braceCount--;
                        if (braceCount == 0)
                        {
                            return i;
                        }

                        break;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Generates the unique identifier.
        /// </summary>
        /// <returns>Count ups</returns>
        private static string GenerateUniqueId()
        {
            // Use a static counter to generate unique IDs
            return (++_idCounter).ToString();
        }
    }
}
