namespace Interpreter.ScriptEngine
{
    internal enum TokenType
    {
        Identifier,      // com, ext, Label, etc.
        Number,          // 123, 45.6 (optional)
        StringLiteral,   // "text" (if you add it later)
        OpenParen,       // (
        CloseParen,      // )
        OpenBrace,       // {
        CloseBrace,      // }
        Semicolon,       // ;
        Dot,             // .
        Comma,           // ,
        KeywordIf,       // if
        KeywordElse,     // else
        Comment,         // --
        Label,           // Label(...)
        Command,        //Command
        Unknown,
        String
    }
}
