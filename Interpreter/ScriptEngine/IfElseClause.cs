/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Interpreter.ScriptEngine/IfElseClause.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace Interpreter.ScriptEngine
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class IfElseClause
    {
        public string Id { get; init; }
        public string Parent { get; init; }
        public string IfClause { get; init; }
        public string ElseClause { get; init; }
        public int Layer { get; init; } // Depth level of the `if-else` structure

        /// <summary>
        ///     Override the ToString method
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Parent: {Parent}, IfClause: {IfClause}, ElseClause: {ElseClause}, Layer: {Layer}";
        }
    }
}
