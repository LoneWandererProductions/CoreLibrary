//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text.RegularExpressions;

//namespace Interpreter.ScriptEngine
//{
//    // Simplified categorized dictionary for demo
//    public class CategorizedDictionary<TKey, TValue> : Dictionary<TKey, (string Category, TValue Value)>
//    {
//        public void Add(string category, TKey key, TValue value)
//        {
//            base.Add(key, (category, value));
//        }
//    }

//    public class IfElseObj
//    {
//        public int Id { get; set; }
//        public int ParentId { get; set; }
//        public int Layer { get; set; }
//        public int Position { get; set; }
//        public bool Else { get; set; }
//        public bool Nested { get; set; }
//        public string Input { get; set; }
//        public CategorizedDictionary<int, string> Commands { get; set; }
//    }

//    internal static class IrtKernel
//    {
//        // Parses input string into blocks: (key, category, value)
//        // Categories: If_Condition, If, Else
//        public static List<(int Key, string Category, string Value)> GetBlocks(string input)
//        {
//            var blocks = new List<(int, string, string)>();

//            input = input.Trim();

//            // Regex to match if condition: if (condition) { ... }
//            // This assumes input starts with 'if'
//            // We'll extract:
//            // 1) Condition
//            // 2) If block commands (inside first {})
//            // 3) Optional else block commands (inside else {...})

//            var ifMatch = Regex.Match(input, @"^if\s*\((.*?)\)\s*\{");
//            if (!ifMatch.Success)
//                throw new ArgumentException("Input does not start with a valid if statement.");

//            string condition = ifMatch.Groups[1].Value.Trim();
//            blocks.Add((0, "If_Condition", condition));

//            // Now parse the blocks inside braces for if and else
//            // Find content inside the first pair of braces after if(condition)
//            int startIndex = ifMatch.Index + ifMatch.Length - 1;
//            int ifBlockEnd = FindMatchingBrace(input, startIndex);
//            string ifBlockContent = input.Substring(startIndex + 1, ifBlockEnd - startIndex - 1).Trim();

//            // Now check for 'else { ... }' after ifBlockEnd
//            int elseIndex = input.IndexOf("else", ifBlockEnd + 1, StringComparison.OrdinalIgnoreCase);

//            string elseBlockContent = null;
//            if (elseIndex >= 0)
//            {
//                // Expect else followed by braces
//                int elseBraceStart = input.IndexOf('{', elseIndex);
//                if (elseBraceStart == -1)
//                    throw new ArgumentException("Else block does not have opening brace.");

//                int elseBraceEnd = FindMatchingBrace(input, elseBraceStart);
//                elseBlockContent = input.Substring(elseBraceStart + 1, elseBraceEnd - elseBraceStart - 1).Trim();
//            }

//            // Now, the ifBlockContent may contain nested ifs or other commands.
//            // To separate commands inside if block, we try to split by semicolon, but
//            // Nested ifs need to be preserved as one command.

//            // We'll parse ifBlockContent for nested ifs.
//            // We iterate, looking for nested 'if' statements and treat them as single commands.

//            var ifCommands = SplitCommandsPreservingIfs(ifBlockContent);
//            int keyCounter = 1;
//            foreach (var cmd in ifCommands)
//            {
//                blocks.Add((keyCounter++, "If", cmd));
//            }

//            if (elseBlockContent != null)
//            {
//                blocks.Add((keyCounter, "Else", elseBlockContent));
//            }

//            return blocks;
//        }

//        // Splits commands by semicolon but preserves nested if blocks as single commands
//        private static List<string> SplitCommandsPreservingIfs(string input)
//        {
//            var commands = new List<string>();

//            int pos = 0;
//            int length = input.Length;
//            while (pos < length)
//            {
//                if (input.Substring(pos).StartsWith("if", StringComparison.OrdinalIgnoreCase))
//                {
//                    // Parse nested if block completely
//                    int ifStart = pos + input.Substring(pos).IndexOf("if", StringComparison.OrdinalIgnoreCase);
//                    int condStart = input.IndexOf('(', ifStart);
//                    if (condStart == -1) throw new ArgumentException("Malformed nested if condition.");

//                    int condEnd = FindMatchingParenthesis(input, condStart);
//                    int braceStart = input.IndexOf('{', condEnd);
//                    if (braceStart == -1) throw new ArgumentException("Malformed nested if block.");

//                    int braceEnd = FindMatchingBrace(input, braceStart);
//                    string nestedIfBlock = input.Substring(ifStart, braceEnd - ifStart + 1);

//                    // Check for optional else after nested if block
//                    int afterBrace = braceEnd + 1;
//                    if (afterBrace < length)
//                    {
//                        // Check if next word is else
//                        var rest = input.Substring(afterBrace).TrimStart();
//                        if (rest.StartsWith("else", StringComparison.OrdinalIgnoreCase))
//                        {
//                            int elseIndex = afterBrace + input.Substring(afterBrace).IndexOf("else", StringComparison.OrdinalIgnoreCase);
//                            int elseBraceStart = input.IndexOf('{', elseIndex);
//                            if (elseBraceStart == -1) throw new ArgumentException("Malformed else block.");

//                            int elseBraceEnd = FindMatchingBrace(input, elseBraceStart);
//                            string elseBlock = input.Substring(elseIndex, elseBraceEnd - elseIndex + 1);
//                            nestedIfBlock += " " + elseBlock;
//                            pos = elseBraceEnd + 1;
//                        }
//                        else
//                        {
//                            pos = braceEnd + 1;
//                        }
//                    }
//                    else
//                    {
//                        pos = braceEnd + 1;
//                    }

//                    commands.Add(nestedIfBlock.Trim());
//                }
//                else
//                {
//                    // Read until next semicolon or end
//                    int semicolonIndex = input.IndexOf(';', pos);
//                    if (semicolonIndex == -1)
//                    {
//                        string cmd = input.Substring(pos).Trim();
//                        if (!string.IsNullOrEmpty(cmd))
//                            commands.Add(cmd);
//                        break;
//                    }
//                    else
//                    {
//                        string cmd = input.Substring(pos, semicolonIndex - pos + 1).Trim();
//                        if (!string.IsNullOrEmpty(cmd))
//                            commands.Add(cmd);
//                        pos = semicolonIndex + 1;
//                    }
//                }
//            }

//            return commands;
//        }

//        // Finds matching closing brace } given position of opening brace {
//        public static int FindMatchingBrace(string text, int openBraceIndex)
//        {
//            if (text[openBraceIndex] != '{') throw new ArgumentException("Expected { at openBraceIndex");

//            int depth = 1;
//            for (int i = openBraceIndex + 1; i < text.Length; i++)
//            {
//                if (text[i] == '{') depth++;
//                else if (text[i] == '}') depth--;
//                if (depth == 0) return i;
//            }
//            throw new ArgumentException("No matching closing brace found.");
//        }

//        // Finds matching closing parenthesis ) given position of opening parenthesis (
//        public static int FindMatchingParenthesis(string text, int openParenIndex)
//        {
//            if (text[openParenIndex] != '(') throw new ArgumentException("Expected ( at openParenIndex");

//            int depth = 1;
//            for (int i = openParenIndex + 1; i < text.Length; i++)
//            {
//                if (text[i] == '(') depth++;
//                else if (text[i] == ')') depth--;
//                if (depth == 0) return i;
//            }
//            throw new ArgumentException("No matching closing parenthesis found.");
//        }

//        // Utility to check for keyword with open paren
//        public static bool ContainsKeywordWithOpenParenthesis(string input, string keyword)
//        {
//            int idx = input.IndexOf(keyword + "(", StringComparison.OrdinalIgnoreCase);
//            return idx >= 0;
//        }

//        // Finds the first index of the keyword in the input
//        public static int FindFirstKeywordIndex(string input, string keyword)
//        {
//            return input.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
//        }
//    }

//    internal static class ConditionalExpressions
//    {
//        public static Dictionary<int, IfElseObj> ParseIfElseClauses(string input)
//        {
//            if (string.IsNullOrEmpty(input))
//            {
//                return null;
//            }

//            var ifElseClauses = new Dictionary<int, IfElseObj>();
//            ProcessInput(input, false, -1, 0, 0, ifElseClauses);
//            return ifElseClauses;
//        }

//        private static void ProcessInput(string input, bool isElse, int parentId, int layer, int position,
//            IDictionary<int, IfElseObj> ifElseClauses)
//        {
//            Trace.WriteLine($"ProcessInput called: layer={layer}, parentId={parentId}, position={position}");
//            Trace.WriteLine($"Input: \"{input}\"");

//            var obj = CreateIfElseObj(input, isElse, parentId, layer, position, ifElseClauses);
//            Trace.WriteLine($"Created IfElseObj: Id={obj.Id}, Layer={obj.Layer}, ParentId={obj.ParentId}");

//            ifElseClauses.Add(obj.Id, obj);

//            var commands = IrtKernel.GetBlocks(input);
//            Trace.WriteLine($"GetBlocks returned {commands.Count} block(s) for input at layer {layer}:");
//            foreach (var (k, c, v) in commands)
//            {
//                Trace.WriteLine($"  Key={k}, Category={c}, Value=\"{v}\"");
//            }

//            obj.Commands ??= new CategorizedDictionary<int, string>();

//            foreach (var (key, category, value) in commands)
//            {
//                if (category.Equals("If", StringComparison.OrdinalIgnoreCase) &&
//                    IrtKernel.ContainsKeywordWithOpenParenthesis(value, "if"))
//                {
//                    // Find nested if block substring starting at 'if'
//                    int nestedIfIndex = IrtKernel.FindFirstKeywordIndex(value, "if");

//                    if (nestedIfIndex != -1)
//                    {
//                        var nestedIfBlock = value.Substring(nestedIfIndex).Trim();
//                        Trace.WriteLine($"Detected nested if-block, recursing into it at layer {layer + 1}");
//                        obj.Nested = true;
//                        ProcessInput(nestedIfBlock, false, obj.Id, layer + 1, key, ifElseClauses);

//                        // Add only the part before nested if block as a separate command if present
//                        string beforeNested = value.Substring(0, nestedIfIndex).Trim();
//                        if (!string.IsNullOrEmpty(beforeNested))
//                        {
//                            obj.Commands.Add(category, key, beforeNested);
//                        }
//                    }
//                    else
//                    {
//                        obj.Commands.Add(category, key, value);
//                    }
//                }
//                else
//                {
//                    obj.Commands.Add(category, key, value);
//                }
//            }
//        }

//        private static IfElseObj CreateIfElseObj(string input, bool isElse, int parentId, int layer, int position,
//            IDictionary<int, IfElseObj> master)
//        {
//            return new IfElseObj
//            {
//                Input = input,
//                Else = isElse,
//                ParentId = parentId,
//                Id = master.Count,
//                Layer = layer,
//                Position = position
//            };
//        }
//    }
//}
