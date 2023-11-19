/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/Irt.cs
 * PURPOSE:     Checks multiple logic errors
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ArrangeBraces_foreach

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Interpreter
{
    /// <summary>
    ///     The Irt class.
    /// </summary>
    internal static class Irt
    {
        /// <summary>
        ///     Parentheses Check
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>Well Formed</returns>
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
        ///     parentheses Check for different Type of  Parenthesis
        /// </summary>
        /// <param name="input">String to check</param>
        /// <param name="openParenthesis">Char that Represents Open</param>
        /// <param name="closeParenthesis">Char that Represents Close</param>
        /// <returns>Well Formed</returns>
        internal static bool CheckMultiple(string input, char[] openParenthesis, char[] closeParenthesis)
        {
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
        ///     Checks if string is well formed, if it is it removes the parenthesis
        ///     Not yet agnostic Char for Open/Close
        /// </summary>
        /// <param name="input">Last bit of string</param>
        /// <param name="closeClause">The close clause.</param>
        /// <param name="openClause">The open clause.</param>
        /// <returns>
        ///     Cleaned String or Error Message
        /// </returns>
        internal static string RemoveParenthesis(string input, char closeClause, char openClause)
        {
            //no Parenthesis? okay we still try to handle it, might be a command with zero parameters
            if (!input.Contains(IrtConst.BaseOpen) && !input.Contains(closeClause))
            {
                return input;
            }

            //check parenthesis, if first is the actual Parenthesis
            if (input.Substring(0, 1) != openClause.ToString())
            {
                return IrtConst.ParenthesisError;
            }

            if (input.Substring(input.Length - 1, 1) != closeClause.ToString())
            {
                return IrtConst.ParenthesisError;
            }

            //remove them
            input = input.Remove(0, 1);
            input = input.Remove(input.Length - 1, 1);
            return input.Trim();
        }

        /// <summary>
        ///     Removes the first symbol.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="symbol">The symbol.</param>
        /// <returns></returns>
        internal static string RemoveFirstSymbol(string input, char symbol)
        {
            return input.Remove(input.IndexOf(symbol.ToString(), StringComparison.Ordinal), 1);
        }

        /// <summary>
        ///     Removes the last symbol.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="symbol">The symbol.</param>
        /// <returns></returns>
        internal static string RemoveLastSymbol(string input, char symbol)
        {
            return input.Substring(0, input.LastIndexOf(symbol.ToString(), StringComparison.Ordinal));
        }

        /// <summary>
        ///     Removes specified part of String
        /// </summary>
        /// <param name="remove">Keyword to remove</param>
        /// <param name="target">target string</param>
        /// <returns>Parameter Part without the Keyword at the front</returns>
        internal static string RemoveWord(string remove, string target)
        {
            target = target.Replace(remove, string.Empty);
            return target.Trim();
        }

        /// <summary>
        ///     For External Commands
        /// </summary>
        /// <param name="input">Command String</param>
        /// <param name="com">Command Register</param>
        /// <returns>Id of Register used</returns>
        internal static int CheckForKeyWord(string input, Dictionary<int, InCommand> com)
        {
            foreach (
                var word in
                com.Where(word => input.StartsWith(word.Value.Command.ToUpper(CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)))
            {
                return word.Key;
            }

            return IrtConst.ErrorParam;
        }

        /// <summary>
        ///     For Internal Commands
        /// </summary>
        /// <param name="input">Command String</param>
        /// <returns>If internal Command was used</returns>
        internal static string CheckInternalCommands(string input)
        {
            foreach (
                var enums in
                IrtConst.InternalCommands.Where(
                    enums => input.StartsWith(enums.ToUpper(CultureInfo.InvariantCulture),
                        StringComparison.Ordinal)))
            {
                return enums;
            }

            return string.Empty;
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
            return paramLst.Where(element => !string.IsNullOrEmpty(element)).ToList();
        }

        /// <summary>
        ///     Makes the Parenthesis well formed e.g.
        ///     (),() and not like () ,   ()
        ///     Not yet in use
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>Well formed Parenthesis</returns>
        internal static string WellFormedParenthesis(string input)
        {
            var regex = new Regex(@"\)(\s*,\s*)\(");
            return regex.Replace(input, string.Empty);
        }

        /// <summary>
        ///     Check the overload and the Parameter Count
        /// </summary>
        /// <param name="command">The command Keyword</param>
        /// <param name="count">The count of Parameters</param>
        /// <param name="commands">The Command Dictionary</param>
        /// <returns>
        ///     The Command Id, identical, if there is no overload, new id, if there is an overload, -1 if something went
        ///     wrong <see cref="int" />.
        /// </returns>
        internal static int CheckOverload(string command, int count, Dictionary<int, InCommand> commands)
        {
            foreach (
                var comm in commands.Where(com => command == com.Value.Command && com.Value.ParameterCount == count))
            {
                return comm.Key;
            }

            return IrtConst.ErrorParam;
        }
    }
}
