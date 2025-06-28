using System;
using System.Collections.Generic;

namespace Interpreter.ScriptEngine
{
    internal sealed class Lexer
    {
        private readonly string _input;
        private int _pos;
        private int _line = 1;
        private int _col = 1;

        private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "if",
            "else"
        };

        public Lexer(string input) => _input = input;

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (!IsAtEnd())
            {
                SkipWhitespace();

                var start = _pos;
                var line = _line;
                var col = _col;

                char c = Peek();

                if (char.IsLetter(c))
                {
                    var ident = ReadWhile(ch => char.IsLetterOrDigit(ch) || ch == '_');
                    if (ident.Equals("Label", StringComparison.OrdinalIgnoreCase) && Peek() == '(')
                    {
                        tokens.Add(new Token { Type = TokenType.Label, Lexeme = ident, Line = line, Column = col });
                    }
                    else if (Keywords.Contains(ident))
                    {
                        tokens.Add(new Token
                        {
                            Type = ident.Equals("if", StringComparison.OrdinalIgnoreCase) ? TokenType.KeywordIf : TokenType.KeywordElse,
                            Lexeme = ident,
                            Line = line,
                            Column = col
                        });
                    }
                    else
                    {
                        tokens.Add(new Token { Type = TokenType.Identifier, Lexeme = ident, Line = line, Column = col });
                    }
                }
                else if (char.IsDigit(c))
                {
                    var number = ReadWhile(char.IsDigit);
                    tokens.Add(new Token { Type = TokenType.Number, Lexeme = number, Line = line, Column = col });
                }
                else if (c == '"')
                {
                    Advance(); // Skip opening quote

                    var stringBuilder = new System.Text.StringBuilder();
                    while (!IsAtEnd() && Peek() != '"')
                    {
                        stringBuilder.Append(Peek()); // Append current character
                        Advance(); // Move to next
                    }

                    if (!IsAtEnd() && Peek() == '"')
                    {
                        Advance(); // Skip closing quote
                        tokens.Add(new Token
                        {
                            Type = TokenType.String,
                            Lexeme = stringBuilder.ToString(),
                            Line = line,
                            Column = col
                        });
                    }
                    else
                    {
                        tokens.Add(new Token
                        {
                            Type = TokenType.Unknown,
                            Lexeme = "\"" + stringBuilder.ToString(), // Include unmatched opening quote
                            Line = line,
                            Column = col
                        });
                    }
                }
                else
                {
                    switch (c)
                    {
                        case ';':
                            Advance(); tokens.Add(Token(TokenType.Semicolon, ";", line, col)); break;
                        case '.':
                            Advance(); tokens.Add(Token(TokenType.Dot, ".", line, col)); break;
                        case ',':
                            Advance(); tokens.Add(Token(TokenType.Comma, ",", line, col)); break;
                        case '(':
                            Advance(); tokens.Add(Token(TokenType.OpenParen, "(", line, col)); break;
                        case ')':
                            Advance(); tokens.Add(Token(TokenType.CloseParen, ")", line, col)); break;
                        case '{':
                            Advance(); tokens.Add(Token(TokenType.OpenBrace, "{", line, col)); break;
                        case '}':
                            Advance(); tokens.Add(Token(TokenType.CloseBrace, "}", line, col)); break;
                        case '-':
                            if (Peek(1) == '-')
                            {
                                Advance(2);
                                var comment = ReadWhile(ch => ch != '\n' && ch != '\r');
                                tokens.Add(new Token { Type = TokenType.Comment, Lexeme = comment.Trim(), Line = line, Column = col });
                            }
                            else
                            {
                                Advance();
                                tokens.Add(Token(TokenType.Unknown, "-", line, col));
                            }
                            break;
                        default:
                            Advance();
                            tokens.Add(Token(TokenType.Unknown, c.ToString(), line, col));
                            break;
                    }
                }
            }

            return tokens;
        }

        private Token Token(TokenType type, string lexeme, int line, int col)
            => new() { Type = type, Lexeme = lexeme, Line = line, Column = col };

        private void SkipWhitespace()
        {
            while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
            {
                if (Peek() == '\n') { _line++; _col = 0; }
                Advance();
            }
        }

        private string ReadWhile(Func<char, bool> predicate)
        {
            var start = _pos;
            while (!IsAtEnd() && predicate(Peek())) Advance();
            return _input.Substring(start, _pos - start);
        }

        private char Peek(int offset = 0) => _pos + offset < _input.Length ? _input[_pos + offset] : '\0';
        private void Advance(int amount = 1) { _pos += amount; _col += amount; }
        private bool IsAtEnd() => _pos >= _input.Length;
    }
}
