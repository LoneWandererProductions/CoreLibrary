/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ParseEngine
 * FILE:        SimpleCommandParser.cs
 * PURPOSE:     Robustly parses commands using a context-aware state machine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using Weaver.Messages;

namespace Weaver.ParseEngine
{
    /// <summary>
    /// Provides robust parsing for commands, handling nested parentheses and quoted strings.
    /// Syntax: [namespace:]command(arg1, arg2).extension(arg1, arg2)
    /// </summary>
    public static class SimpleCommandParser
    {
        /// <summary>
        /// Parses a raw command string into a <see cref="ParsedCommand" /> structure.
        /// </summary>
        /// <param name="raw">The raw input text.</param>
        /// <returns>The parsed command object.</returns>
        public static ParsedCommand Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new FormatException("Empty input.");

            raw = raw.Trim();

            // 1. Split Extension
            // We search for the last dot '.' that is NOT inside quotes or parentheses.
            // This handles cases like: system:delete(file.txt).log()
            var dotIndex = FindSeparatorIndex(raw, '.', fromRight: true);

            string mainPart;
            string extPart = string.Empty;

            if (dotIndex != -1)
            {
                mainPart = raw.Substring(0, dotIndex).Trim();
                extPart = raw.Substring(dotIndex + 1).Trim();
            }
            else
            {
                mainPart = raw;
            }

            // 2. Split Namespace
            // We search for the first colon ':' that is NOT inside quotes or parentheses.
            var colonIndex = FindSeparatorIndex(mainPart, ':');
            string ns = string.Empty;
            string cmdSig;

            if (colonIndex != -1)
            {
                ns = mainPart.Substring(0, colonIndex).Trim();
                cmdSig = mainPart.Substring(colonIndex + 1).Trim();
            }
            else
            {
                cmdSig = mainPart;
            }

            // 3. Parse Command Signature "Name(Args)"
            var (cmdName, cmdArgs) = ParseSignature(cmdSig);

            // Validate Command Name
            if (string.IsNullOrEmpty(cmdName))
                throw new FormatException($"Invalid command format: '{mainPart}'");

            // Validate format "invalid(" -> ParseSignature throws or returns weird state
            // If the input had an opening parenthesis but we couldn't parse it cleanly, ParseSignature handles checks.

            // 4. Parse Extension Signature "Name(Args)" (if exists)
            string extName = string.Empty;
            string[] extArgs = Array.Empty<string>();

            if (!string.IsNullOrEmpty(extPart))
            {
                var parsedExt = ParseSignature(extPart);
                extName = parsedExt.Name;
                extArgs = parsedExt.Args;

                if (string.IsNullOrEmpty(extName))
                    throw new FormatException($"Invalid extension format: '{extPart}'");
            }

            return new ParsedCommand
            {
                Namespace = ns,
                Name = cmdName,
                Args = cmdArgs,
                Extension = extName,
                ExtensionArgs = extArgs
            };
        }

        /// <summary>
        /// Parses a signature string like "Command(Arg1, Arg2)" into its Name and Arguments.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Get command name and parameter</returns>
        /// <exception cref="System.FormatException">Missing closing parenthesis in '{input}'.</exception>
        private static (string Name, string[] Args) ParseSignature(string input)
        {
            // Find the first '(' that isn't inside quotes.
            var openParen = FindFirstOpenParen(input);

            if (openParen == -1)
            {
                // No parentheses -> Command with 0 args, e.g. "help" (if supported) or "echo" vs "echo()"
                // Note: Your tests use "echo()", which has parens.
                // If input is "invalid(", FindFirstOpenParen returns index of '('.
                return (input.Trim(), Array.Empty<string>());
            }

            var name = input.Substring(0, openParen).Trim();

            // Validate closing parenthesis
            var closeParen = input.LastIndexOf(')');

            // If we have an open paren but no close paren, or close is before open
            if (closeParen == -1 || closeParen < openParen)
                throw new FormatException($"Missing closing parenthesis in '{input}'.");

            // Extract content inside ()
            var inner = input.Substring(openParen + 1, closeParen - openParen - 1);

            // Split arguments by comma
            var args = SmartSplit(inner, ',');

            return (name, args.ToArray());
        }

        /// <summary>
        /// Finds the index of the first '(' that is not inside quotes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Index of first open Parenthesis.</returns>
        private static int FindFirstOpenParen(string input)
        {
            var inDoubleQuote = false;
            var inSingleQuote = false;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
                else if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;

                if (!inDoubleQuote && !inSingleQuote && c == '(')
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of a separator character, ignoring those inside quotes or parentheses.
        /// Useful for finding '.' (Extension) or ':' (Namespace).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="target">The target.</param>
        /// <param name="fromRight">if set to <c>true</c> [from right].</param>
        /// <returns>Find index of Separator.</returns>
        private static int FindSeparatorIndex(string input, char target, bool fromRight = false)
        {
            var inDoubleQuote = false;
            var inSingleQuote = false;
            var parenDepth = 0;

            var start = fromRight ? input.Length - 1 : 0;
            var end = fromRight ? -1 : input.Length;
            var step = fromRight ? -1 : 1;

            for (var i = start; i != end; i += step)
            {
                var c = input[i];

                // Handle Quotes
                if (c == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
                else if (c == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;

                if (inDoubleQuote || inSingleQuote) continue;

                // Handle Parentheses
                // Note: When scanning backwards, ')' INCREASES depth (entering a group), '(' DECREASES it.
                if (c == ')') parenDepth += fromRight ? 1 : -1;
                else if (c == '(') parenDepth += fromRight ? -1 : 1;

                // Match Target
                // We only match if we are at the top level (depth 0) AND the character matches.
                // We do this check *after* depth adjustment logic would technically be cleaner, 
                // but since separator chars ('.' / ':') are never '(' or ')', this order is safe.
                if (parenDepth == 0 && c == target)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Splits a string by a separator, respecting quotes and parentheses.
        /// Removes surrounding quotes from the resulting parts.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        private static List<string> SmartSplit(string input, char separator)
        {
            var parts = new List<string>();
            if (string.IsNullOrWhiteSpace(input)) return parts;

            var current = new StringBuilder();
            var inDoubleQuote = false;
            var inSingleQuote = false;
            var parenDepth = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                // Toggle Quotes
                if (c == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    current.Append(c);
                    continue;
                }

                if (c == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    current.Append(c);
                    continue;
                }

                // If inside quotes, just accumulate
                if (inDoubleQuote || inSingleQuote)
                {
                    current.Append(c);
                    continue;
                }

                // Handle nesting
                if (c == '(') parenDepth++;
                else if (c == ')') parenDepth--;

                // Check for Split
                if (parenDepth == 0 && c == separator)
                {
                    // Add token
                    parts.Add(StripQuotes(current.ToString().Trim()));
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            // Add the last part
            // Note: If input ends with separator, or we have content, add it.
            // For "a,b", loop finishes after 'b'. We add 'b'.
            // For empty input, we returned early.
            // For " ", Trim makes it empty string.
            var lastPart = current.ToString().Trim();
            if (parts.Count > 0 || !string.IsNullOrEmpty(lastPart))
            {
                parts.Add(StripQuotes(lastPart));
            }

            return parts;
        }

        /// <summary>
        /// Helper to remove surrounding quotes from a string, if present.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>String without quotes.</returns>
        private static string StripQuotes(string val)
        {
            if (string.IsNullOrEmpty(val)) return val;
            if (val.Length >= 2)
            {
                if (val.StartsWith("\"") && val.EndsWith("\"")) return val[1..^1];
                if (val.StartsWith("'") && val.EndsWith("'")) return val[1..^1];
            }

            return val;
        }
    }
}
