/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        TokenType.cs
 * PURPOSE:     Collection of all possible token types in the script engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Collection of all possible token types in the script engine.
    /// </summary>
    internal enum TokenType
    {
        Identifier = 0, // com, ext, Label, etc.
        Number = 1, // 123, 45.6 (optional)
        StringLiteral = 2, // "text" (if you add it later), TODO check if used
        OpenParen = 3, // (
        CloseParen = 4, // )
        OpenBrace = 5, // {
        CloseBrace = 6, // }
        Semicolon = 7, // ;
        Dot = 8, // .
        Comma = 9, // ,
        KeywordIf = 10, // if
        KeywordElse = 11, // else
        Comment = 12, // --
        Label = 13, // Label(...)
        Command = 14, //Command, TODO check if used
        Unknown = 15,
        Keyword = 16,
        KeywordGoto = 17,
        String = 18,
        Plus = 19,
        Minus = 20,
        Star = 21,
        Slash = 22,
        Greater = 23,
        GreaterEqual = 24,
        Less = 25,
        LessEqual = 26,
        Equal = 27,
        EqualEqual = 28,
        Bang = 29,
        BangEqual = 30,
        KeywordDo = 31,
        KeywordWhile = 32,
        LogicalAnd = 33,
        BitAnd = 34,
        LogicalOr = 35,
        BitOr = 36
    }
}