/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtKernel.cs
 * PURPOSE:     Checks multiple logic errors, handle most atomic operations for the Interpreter. If something is broken here everything is broken
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ArrangeBraces_foreach
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExtendedSystemObjects;
using Interpreter.Resources;

namespace Interpreter
{
    /// <summary>
    ///     The Irt class.
    /// </summary>
    internal static class IrtKernel
    {
        /// <summary>
        ///     Validates the parameters. Parameter Count
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="parametersCount">Count of parameter</param>
        /// <param name="commands">The commands.</param>
        /// <returns>If Parameters are correct</returns>
        internal static bool ValidateParameters(int key, int parametersCount,
            IReadOnlyDictionary<int, InCommand> commands)
        {
            // Check if the command requires parameters or if the input has correctly formatted single parameter
            return commands[key].ParameterCount == parametersCount;
        }

        /// <summary>
        ///     Checks if the input string has balanced parentheses of a single type.
        /// </summary>
        /// <param name="input">Input string to check.</param>
        /// <returns>True if parentheses are balanced, false otherwise.</returns>
        /// <example>
        ///     <code>
        /// bool result = Irt.SingleCheck("(a + b) * c");
        /// // result is true
        /// </code>
        /// </example>
        internal static bool SingleCheck(string input)
        {
            // Index of the currently open parentheses:
            var parentheses = new Stack<int>();

            foreach (var chr in input)
            {
                if (chr == IrtConst.BaseOpen)
                {
                    parentheses.Push(0); // Add index to stack
                }
                // Check if the 'chr' is a close parenthesis, and get its index:
                else
                {
                    if (chr != IrtConst.BaseClose)
                    {
                        continue;
                    }

                    // Return 'false' if the stack is empty or if the currently
                    if (parentheses.Count == 0)
                    {
                        return false;
                    }

                    // open parenthesis is not paired with the 'chr':
                    if (parentheses.Pop() != 0)
                    {
                        return false;
                    }
                }
            }

            // Return 'true' if there is no open parentheses, and 'false' - otherwise:
            return parentheses.Count == 0;
        }

        /// <summary>
        ///     Checks if the input string has balanced parentheses of multiple types.
        /// </summary>
        /// <param name="input">Input string to check.</param>
        /// <param name="openParenthesis">Array of opening parentheses characters.</param>
        /// <param name="closeParenthesis">Array of closing parentheses characters.</param>
        /// <returns>True if parentheses are balanced, false otherwise.</returns>
        internal static bool CheckMultipleParenthesis(string input, char[] openParenthesis, char[] closeParenthesis)
        {
            //Open and close parentheses arrays are not the same length.
            if (openParenthesis.Length != closeParenthesis.Length)
            {
                return false;
            }

            // Index of the currently open parentheses:
            var parentheses = new Stack<int>();

            foreach (var chr in input)
            {
                int index;

                // Check if the 'chr' is an open parenthesis, and get its index:
                if ((index = Array.IndexOf(openParenthesis, chr)) != -1)
                {
                    parentheses.Push(index); // Add index to stack
                }
                // Check if the 'chr' is a close parenthesis, and get its index:
                else
                {
                    if ((index = Array.IndexOf(closeParenthesis, chr)) == -1)
                    {
                        continue;
                    }

                    // Return 'false' if the stack is empty or if the currently
                    // open parenthesis is not paired with the 'chr':
                    if (parentheses.Count == 0 || parentheses.Pop() != index)
                    {
                        return false;
                    }
                }
            }

            // Return 'true' if there is no open parentheses, and 'false' - otherwise:
            return parentheses.Count == 0;
        }

        /// <summary>
        ///     Removes the outermost parentheses from the input string if well-formed.
        /// </summary>
        /// <param name="input">Last bit of string</param>
        /// <param name="openClause">The open clause.</param>
        /// <param name="closeClause">The close clause.</param>
        /// <returns>
        ///     Cleaned String or Error Message
        /// </returns>
        internal static string RemoveParenthesis(string input, char openClause, char closeClause)
        {
            //no Parenthesis? okay we still try to handle it, might be a command with zero parameters
            if (!input.Contains(IrtConst.BaseOpen) && !input.Contains(closeClause))
            {
                return input;
            }

            // Ensure the string starts with the openClause and ends with the closeClause
            return StartsAndEndsWith(input, openClause, closeClause)
                ? input.Substring(1, input.Length - 2).Trim()
                : IrtConst.ParenthesisError;
        }

        /// <summary>
        ///     Removes the first occurrence of the specified symbol from the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="symbol">The symbol to remove.</param>
        /// <returns>The modified string.</returns>
        internal static string RemoveFirstOccurrence(string input, char symbol)
        {
            return input.Remove(input.IndexOf(symbol.ToString(), StringComparison.Ordinal), 1);
        }

        /// <summary>
        ///     Removes at the last matched symbol.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The modified string.</returns>
        internal static string CutLastOccurrence(string input, char symbol)
        {
            return input.Substring(0, input.LastIndexOf(symbol.ToString(), StringComparison.Ordinal));
        }

        /// <summary>
        ///     Removes specified part of String
        /// </summary>
        /// <param name="remove">Keyword to remove</param>
        /// <param name="input">input string</param>
        /// <returns>Parameter Part without the Keyword at the front</returns>
        internal static string RemoveWord(string remove, string input)
        {
            // Find the index of remove in target, ignoring case
            var index = input.IndexOf(remove, StringComparison.OrdinalIgnoreCase);

            // If remove is found, remove it from target
            if (index != IrtConst.Error)
            {
                input = input.Remove(index, remove.Length);
            }

            return input.Trim();
        }

        /// <summary>
        ///     For External Commands
        /// </summary>
        /// <param name="input">Command String</param>
        /// <param name="com">Command Register</param>
        /// <returns>Id of Register used, if nothing was found, return -1.</returns>
        internal static int CheckForKeyWord(string input, Dictionary<int, InCommand> com)
        {
            //just for the compare, make it to upper do not change the input string
            input = input.ToUpperInvariant();

            if (input.Contains(IrtConst.AdvancedOpen))
            {
                var index = input.IndexOf(IrtConst.AdvancedOpen);

                if (index >= 0)
                {
                    input = input[..index];
                    input = input.Trim();
                }
            }
            else if (input.Contains(IrtConst.BaseOpen))
            {
                var index = input.IndexOf(IrtConst.BaseOpen);

                if (index >= 0)
                {
                    input = input[..index];
                    input = input.Trim();
                }
            }

            foreach (var (key, inCommand) in com)
            {
                if (string.Equals(input.ToUpperInvariant(), inCommand.Command.ToUpperInvariant(),
                        StringComparison.Ordinal))
                {
                    return
                        key;
                }
            }

            return IrtConst.Error;
        }

        /// <summary>
        ///     Get the Parameter as list
        /// </summary>
        /// <param name="parameterPart">Parameter String</param>
        /// <param name="splitter">the char we split</param>
        /// <returns>Splits Parameter Part by Splitter</returns>
        internal static List<string> SplitParameter(string parameterPart, char splitter)
        {
            var lst = parameterPart.Split(splitter).ToList();
            var paramLst = new List<string>(lst.Count);

            paramLst.AddRange(lst.Select(param => param.Trim()));

            // remove empty trash
            paramLst = paramLst.Where(element => !string.IsNullOrEmpty(element)).ToList();

            //remove all empty Parameters
            return (from item in paramLst
                let result = Regex.Replace(item, IrtConst.RegexRemoveWhiteSpace, string.Empty)
                where result != IrtConst.EmptyParameter
                select item).ToList();
        }

        /// <summary>
        ///     Makes the Parenthesis well formed e.g.
        ///     (),() and not like () ,   (), also handles all cases for {}
        ///     Not yet in use
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>Well formed string with Parenthesis</returns>
        internal static string WellFormedParenthesis(string input)
        {
            input = input.Trim();
            var regex = Regex.Replace(input, IrtConst.RegexParenthesisWellFormedPatternLeft, string.Empty);
            return Regex.Replace(regex, IrtConst.RegexParenthesisWellFormedPatternRight, string.Empty);
        }

        /// <summary>
        ///     Check the overload and the Parameter Count
        ///     if count is positive count and parameter must be equal.
        ///     if count is negative the parameter count must be equal and or bigger
        /// </summary>
        /// <param name="command">The command Keyword</param>
        /// <param name="count">The count of Parameters</param>
        /// <param name="commands">The Command Dictionary</param>
        /// <returns>
        ///     The Command Id, identical, if there is no overload, new id, if there is an overload, null if something went
        ///     wrong <see cref="int" />.
        /// </returns>
        internal static int? CheckOverload(string command, int count, Dictionary<int, InCommand> commands)
        {
            foreach (var (key, value) in commands.Where(comm => command == comm.Value.Command))
            {
                if (value.ParameterCount < 0)
                {
                    if (count >= Math.Abs(value.ParameterCount))
                    {
                        return key;
                    }
                }
                else
                {
                    if (value.ParameterCount == count)
                    {
                        return key;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Processes the parameters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="key">The key.</param>
        /// <param name="commands">The commands in use.</param>
        /// <returns>return Parameter</returns>
        internal static (int Status, string Parameter) GetParameters(string input, int key,
            IReadOnlyDictionary<int, InCommand> commands)
        {
            var command = commands[key].Command.ToUpperInvariant();
            var parameterPart = RemoveWord(command, input);

            return parameterPart.StartsWith(IrtConst.AdvancedOpen)
                ? (IrtConst.BatchCommand, parameterPart)
                : (IrtConst.ParameterCommand,
                    RemoveParenthesis(parameterPart, IrtConst.BaseOpen, IrtConst.BaseClose));
        }

        /// <summary>
        ///     Checks if the input string follows the format of "command(label)" with optional whitespace and case insensitivity.
        /// </summary>
        /// <param name="input">The input string to check.</param>
        /// <param name="command">The expected command.</param>
        /// <param name="label">The expected label.</param>
        /// <returns>True if the string matches the format; otherwise, false.</returns>
        internal static bool CheckFormat(string input, string command, string label)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // Trim the input to remove leading and trailing whitespace
            var trimmedInput = input.Trim();

            // Convert to upper case for case-insensitive comparison
            var upperInput = trimmedInput.ToUpperInvariant();
            var upperCommand = command.ToUpperInvariant();
            var upperLabel = label.ToUpperInvariant();

            // Check if it starts with "command(" and ends with ")"
            var start = upperCommand + IrtConst.BaseOpen;
            var end = IrtConst.BaseClose.ToString();

            if (!upperInput.StartsWith(start, StringComparison.Ordinal) ||
                !upperInput.EndsWith(end, StringComparison.Ordinal))
            {
                return false;
            }

            // Extract the content within the parentheses
            var contentStartIndex = upperCommand.Length + 1;
            var contentLength = upperInput.Length - contentStartIndex - 1;
            var content = upperInput.Substring(contentStartIndex, contentLength).Trim();

            // Check if the content matches the expected label
            return content == upperLabel;
        }

        /// <summary>
        ///     Extracts the condition from an 'if' statement.
        /// </summary>
        /// <param name="input">The input string containing the 'if' statement.</param>
        /// <param name="keyword">The keyword to remove from the start of the string.</param>
        /// <returns>The extracted condition string.</returns>
        internal static string ExtractCondition(string input, string keyword)
        {
            // Trim leading and trailing whitespace from the input
            input = input.Trim();

            // Remove the keyword if it appears at the start of the input (e.g., "if")
            if (input.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(keyword.Length).Trim();
            }

            // Find the opening and closing parenthesis around the condition
            var openParenIndex = input.IndexOf(IrtConst.BaseOpen); // '('
            var closeParenIndex = input.IndexOf(IrtConst.BaseClose); // ')'

            // If either of the parentheses are missing, return an empty condition
            if (openParenIndex == IrtConst.Error || closeParenIndex == IrtConst.Error ||
                closeParenIndex < openParenIndex)
            {
                return string.Empty;
            }

            // Extract the condition between the parentheses
            return input.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();
        }

        /// <summary>
        ///     Finds the index of the first occurrence of the given keyword followed by an open parenthesis.
        /// </summary>
        /// <param name="input">The input string to search within.</param>
        /// <param name="keyword">The keyword to find.</param>
        /// <returns>The index of the keyword if found, or -1 if not found.</returns>
        internal static int FindFirstKeywordIndex(string input, string keyword)
        {
            if (string.IsNullOrEmpty(input))
            {
                return IrtConst.Error;
            }

            var position = input.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

            while (position != IrtConst.Error)
            {
                var openParenIndex = position + keyword.Length;
                while (openParenIndex < input.Length && char.IsWhiteSpace(input[openParenIndex]))
                {
                    openParenIndex++;
                }

                if ((openParenIndex < input.Length && input[openParenIndex] == IrtConst.BaseOpen) ||
                    (openParenIndex < input.Length && input[openParenIndex] == IrtConst.AdvancedOpen))
                {
                    return position;
                }

                position = input.IndexOf(keyword, position + 1, StringComparison.OrdinalIgnoreCase);
            }

            return IrtConst.Error;
        }

        /// <summary>
        ///     Checks if the input string contains a keyword followed by an open parenthesis.
        /// </summary>
        /// <param name="input">The input string to check.</param>
        /// <param name="keyword">The keyword to look for.</param>
        /// <returns>True if the keyword followed by an open parenthesis is found, otherwise false.</returns>
        internal static bool ContainsKeywordWithOpenParenthesis(string input, string keyword)
        {
            return FindFirstKeywordIndex(input, keyword) != -1;
        }

        /// <summary>
        ///     Extracts the first If-Else block and the position of the 'else' keyword from the input string.
        /// </summary>
        /// <param name="input">The input string containing the If-Else structure.</param>
        /// <returns>A tuple containing the extracted If-Else block and the position of the 'else' keyword.</returns>
        internal static (string block, int elsePosition) ExtractFirstIfElse(string input)
        {
            var start = FindFirstKeywordIndex(input, "if");
            if (start == -1)
            {
                return (null, -1);
            }

            var isMatch = IsValidIfStatement(input);
            if (!isMatch)
            {
                return (null, -1);
            }

            var end = start;
            var braceCount = 0;
            var elseFound = false;
            var elsePosition = -1;
            //todo overhaul
            var containsElse = input.Contains("else", StringComparison.OrdinalIgnoreCase);

            for (var i = start; i < input.Length; i++)
            {
                var cursor = input[i];

                if (cursor == '{')
                {
                    braceCount++;
                }
                else if (cursor == '}')
                {
                    braceCount--;
                    if (braceCount != 0)
                    {
                        continue;
                    }

                    if (containsElse)
                    {
                        continue;
                    }

                    end = i;
                    break;
                }
                else if (i + 4 <= input.Length &&
                         IsKeywordAt(input, i, "else") && braceCount == 0)
                {
                    elseFound = true;
                    elsePosition = i;
                    var index = input.IndexOf("else", i, StringComparison.OrdinalIgnoreCase);
                    end = index + 4;
                    break;
                }
            }

            if (elseFound)
            {
                if (end == input.Length)
                {
                    return (input.Substring(start, input.Length), elsePosition);
                }

                for (var i = end; i < input.Length; i++)
                {
                    var cursor = input[i];

                    if (cursor == '{')
                    {
                        braceCount++;
                    }
                    else if (cursor == '}')
                    {
                        braceCount--;

                        if (braceCount != 0)
                        {
                            continue;
                        }

                        end = i;
                        break;
                    }
                    else if (cursor == ';')
                    {
                        if (braceCount != 0)
                        {
                            continue;
                        }

                        end = i;
                        break;
                    }
                }
            }

            return (input.Substring(start, end - start + 1).Trim(), elsePosition);
        }

        /// <summary>
        ///     Determines whether [is keyword at] [the specified input].
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="index">The index.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        ///     <c>true</c> if [is keyword at] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsKeywordAt(string input, int index, string keyword)
        {
            if (index < 0 || index + keyword.Length > input.Length)
            {
                return false;
            }

            return string.Compare(input, index, keyword, 0, keyword.Length, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        ///     Gets the blocks.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>CategorizedDictionary with commands and if blocks</returns>
        internal static CategorizedDictionary<int, string> GetBlocks(string input)
        {
            var formattedBlocks = new CategorizedDictionary<int, string>();

            while (!string.IsNullOrWhiteSpace(input))
            {
                var ifIndex = FindFirstKeywordIndex(input, IrtConst.InternalIf);

                if (ifIndex == IrtConst.Error)
                {
                    // Handle remaining input as a block
                    GenerateCommandBlock(input, ref formattedBlocks);
                    break;
                }

                // Process content before the 'if'
                var beforeIf = input.Substring(0, ifIndex).Trim();
                if (!string.IsNullOrWhiteSpace(beforeIf))
                {
                    GenerateCommandBlock(beforeIf, ref formattedBlocks);
                }

                // Update input to start from the 'if'
                input = input.Substring(ifIndex);

                var (ifElseBlock, elsePosition) = ExtractFirstIfElse(input);
                if (ifElseBlock != null)
                {
                    if (elsePosition == IrtConst.Error)
                    {
                        // Handle 'if' block
                        var condition = ExtractCondition(ifElseBlock, "if");
                        formattedBlocks.Add("If_Condition", formattedBlocks.Count, condition);
                        var ifBlock = RemoveCondition(ifElseBlock, "If");
                        ifBlock = RemoveParenthesis(ifBlock, IrtConst.AdvancedOpen, IrtConst.AdvancedClose);
                        formattedBlocks.Add("If", formattedBlocks.Count, ifBlock);
                    }
                    else
                    {
                        // Handle 'if' and 'else' blocks
                        var ifBranch = ifElseBlock.Substring(0, elsePosition).Trim();
                        var condition = ExtractCondition(ifBranch, "if");
                        formattedBlocks.Add("If_Condition", formattedBlocks.Count, condition);
                        var ifBlock = RemoveCondition(ifBranch, "If");
                        ifBlock = RemoveParenthesis(ifBlock, IrtConst.AdvancedOpen, IrtConst.AdvancedClose);
                        formattedBlocks.Add("If", formattedBlocks.Count, ifBlock);

                        var elseBranch = ifElseBlock.Substring(elsePosition).Trim();
                        elseBranch = RemoveWord("else", elseBranch);
                        elseBranch = RemoveParenthesis(elseBranch, IrtConst.AdvancedOpen, IrtConst.AdvancedClose);
                        formattedBlocks.Add("Else", formattedBlocks.Count, elseBranch);
                    }

                    // Update input to remove processed block
                    input = input.Substring(ifElseBlock.Length).Trim();
                }
                else
                {
                    formattedBlocks.Add("Error", formattedBlocks.Count, input);
                    input = string.Empty;
                }
            }

            return formattedBlocks;
        }

        /// <summary>
        ///     Determines the command index based on the input string and a dictionary of commands.
        /// </summary>
        /// <param name="input">The input string to check against commands.</param>
        /// <param name="commands">A dictionary of commands to match against.</param>
        /// <returns>The index of the matching command, or an error code if no match is found.</returns>
        internal static int GetCommandIndex(string input, Dictionary<int, InCommand> commands)
        {
            input = input.ToUpperInvariant();

            if (input.Contains(IrtConst.BaseOpen))
            {
                var index = input.IndexOf(IrtConst.BaseOpen);
                if (index >= 0)
                {
                    input = input.Substring(0, index).Trim();
                }
            }

            if (input.Contains(IrtConst.AdvancedOpen))
            {
                var index = input.IndexOf(IrtConst.AdvancedOpen);
                if (index >= 0)
                {
                    input = input.Substring(0, index).Trim();
                }
            }

            foreach (var (key, command) in commands)
                if (string.Equals(input, command.Command, StringComparison.OrdinalIgnoreCase))
                {
                    return key;
                }

            return IrtConst.Error;
        }

        /// <summary>
        ///     Generates the command block.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="formattedBlocks">The formatted blocks.</param>
        private static void GenerateCommandBlock(string input, ref CategorizedDictionary<int, string> formattedBlocks)
        {
            foreach (var command in SplitParameter(input, IrtConst.NewCommand).ToList())
            {
                var type = GetCommandType(command);
                formattedBlocks.Add(type, formattedBlocks.Count, command);
            }
        }

        /// <summary>
        ///     Removes the condition.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        private static string RemoveCondition(string input, string keyword)
        {
            // Step 1: Remove the keyword (e.g., "if" or "do")
            input = RemoveWord(keyword, input).Trim();

            // Step 2: Find and remove the first set of parentheses and the condition within it
            var openParenIndex = input.IndexOf('(');
            if (openParenIndex == IrtConst.Error)
            {
                return input;
            }

            var closeParenIndex = input.IndexOf(')', openParenIndex);

            if (closeParenIndex != IrtConst.Error)
            {
                // Remove everything from the open parenthesis to the close parenthesis
                input = input.Remove(openParenIndex, closeParenIndex - openParenIndex + 1).Trim();
            }

            return input;
        }


        /// <summary>
        ///     Determines whether [is valid if statement] [the specified input].
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///     <c>true</c> if [is valid if statement] [the specified input]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidIfStatement(string input)
        {
            var openParenthesis = new[] { IrtConst.BaseOpen, IrtConst.AdvancedOpen };
            var closeParenthesis = new[] { IrtConst.BaseClose, IrtConst.AdvancedClose };

            // Ensure parentheses are balanced
            if (!CheckMultipleParenthesis(input, openParenthesis, closeParenthesis))
            {
                return false;
            }

            // Check if the string starts with "if(" and ends with ")"
            if (!input.StartsWith(IrtConst.InternalIf, StringComparison.CurrentCultureIgnoreCase) ||
                input.EndsWith(IrtConst.BaseClose.ToString(), StringComparison.Ordinal))
            {
                return false;
            }

            // Extract the content between "if(" and ")"
            var condition = ExtractCondition(input, IrtConst.InternalIf);

            // Check if the extracted content is not empty
            return !string.IsNullOrEmpty(condition);
        }

        /// <summary>
        ///     Gets the type of the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The Command Type</returns>
        private static string GetCommandType(string command)
        {
            command = command.Trim();
            var keywordIndex = GetCommandIndex(command, IrtConst.InternContainerCommands);
            return keywordIndex == -1 ? "Command" : IrtConst.InternContainerCommands[keywordIndex].Command;
        }

        /// <summary>
        ///     Checks if the string starts and ends with the specified characters.
        /// </summary>
        /// <param name="input">Input string to check.</param>
        /// <param name="start">Expected starting character.</param>
        /// <param name="end">Expected ending character.</param>
        /// <returns>True if the string starts with 'start' and ends with 'end', false otherwise.</returns>
        private static bool StartsAndEndsWith(string input, char start, char end)
        {
            return input.Length > 1 && input[0] == start && input[^1] == end;
        }
    }
}
