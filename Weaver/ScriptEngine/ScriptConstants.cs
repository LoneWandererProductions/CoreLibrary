/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        ScriptConstants.cs
 * PURPOSE:     Central constants for script parsing
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Holds all reusable constants used in the script engine lexer and parser.
    /// </summary>
    internal static class ScriptConstants
    {
        /// <summary>
        /// If Keywords.
        /// </summary>
        internal const string If = "if";

        /// <summary>
        /// The else Keywords.
        /// </summary>
        internal const string Else = "else";

        /// <summary>
        /// The label Keywords.
        /// </summary>
        internal const string Label = "label";

        /// <summary>
        /// The goto Keywords.
        /// </summary>
        internal const string Goto = "goto";

        /// <summary>
        /// The do Keywords.
        /// </summary>
        internal const string Do = "do";

        /// <summary>
        /// The while Keywords.
        /// </summary>
        internal const string While = "while";

        /// <summary>
        /// If open token
        /// </summary>
        internal const string IfOpenToken = "If_Open";

        /// <summary>
        /// If end token
        /// </summary>
        internal const string IfEndToken = "If_End";

        /// <summary>
        /// The goto token
        /// </summary>
        internal const string GotoToken = "Goto";

        /// <summary>
        /// The label token
        /// </summary>
        internal const string LabelToken = "Label";

        /// <summary>
        /// The do open token
        /// </summary>
        internal const string DoOpenToken = "Do_Open";

        /// <summary>
        /// The do end token
        /// </summary>
        internal const string DoEndToken = "Do_End";

        /// <summary>
        /// If condition token
        /// </summary>
        internal const string IfConditionToken = "If_Condition";

        /// <summary>
        /// The while condition token
        /// </summary>
        internal const string WhileConditionToken = "While_Condition";

        /// <summary>
        /// The command token
        /// </summary>
        internal const string CommandToken = "Command";

        /// <summary>
        /// The command rewrite token
        /// </summary>
        internal const string CommandRewriteToken = "Command_Rewrite";

        /// <summary>
        /// Else open token
        /// </summary>
        internal const string ElseOpenToken = "Else_Open";

        /// <summary>
        /// Else end token
        /// </summary>
        internal const string ElseEndToken = "Else_End";

        /// <summary>
        /// The assignment token
        /// </summary>
        internal const string AssignmentToken = "Assignment";

        // Operators / punctuation
        internal const string Semicolon = ";";
        internal const string Dot = ".";
        internal const string Comma = ",";
        internal const string OpenParen = "(";
        internal const string CloseParen = ")";
        internal const string OpenBrace = "{";
        internal const string CloseBrace = "}";
        internal const string Minus = "-";
        internal const string Plus = "+";
        internal const string Star = "*";
        internal const string Slash = "/";
        internal const string Equal = "=";
        internal const string EqualEqual = "==";
        internal const string Bang = "!";
        internal const string BangEqual = "!=";
        internal const string Greater = ">";
        internal const string GreaterEqual = ">=";
        internal const string Less = "<";
        internal const string LessEqual = "<=";
        internal const string LogicalAndSymbol = "&&";
        internal const string LogicalOrSymbol = "||";
        internal const string LogicalNotSymbol = "!";

        internal const string LogicalAndWord = "and";
        internal const string LogicalOrWord = "or";
        internal const string LogicalNotWord = "not";
    }
}