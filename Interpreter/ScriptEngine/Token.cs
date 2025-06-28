namespace Interpreter.ScriptEngine
{
    internal sealed class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString() => $"{Type}: '{Lexeme}' (Line {Line}, Col {Column})";
    }

}
