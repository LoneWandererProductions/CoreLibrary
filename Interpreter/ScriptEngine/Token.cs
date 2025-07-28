/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Token.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Interpreter.ScriptEngine;

internal sealed class Token
{
    public TokenType Type { get; init; }
    public string Lexeme { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString()
    {
        return $"{Type}: '{Lexeme}' (Line {Line}, Col {Column})";
    }
}
