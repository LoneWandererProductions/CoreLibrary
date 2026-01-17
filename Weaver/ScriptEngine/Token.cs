/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Token.cs
 * PURPOSE:     Our Token representation for the script engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Represents a lexical token produced by the script engine.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    internal sealed class Token
    {
        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        public TokenType Type { get; init; }

        /// <summary>
        /// Gets the raw lexeme of the token as found in the source.
        /// </summary>
        public string? Lexeme { get; init; }

        /// <summary>
        /// Gets the line number in the source where the token appears (1-based).
        /// </summary>
        public int Line { get; init; }

        /// <summary>
        /// Gets the column number in the source where the token starts (1-based).
        /// </summary>
        public int Column { get; init; }

        /// <summary>
        /// Returns a human-readable string representation of the token.
        /// Example: Identifier: 'foo' (Line 1, Col 5)
        /// </summary>
        public override string ToString()
        {
            return $"{Type}: '{Lexeme}' (Line {Line}, Col {Column})";
        }
    }
}