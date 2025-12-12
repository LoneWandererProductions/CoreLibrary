/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lexer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using System.Text;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// A lexical analyzer that splits a script into its smallest meaningful components (tokens).
    /// Supports identifiers, keywords, numbers, strings, operators, and comments.
    /// </summary>
    internal sealed class Lexer
    {
        /// <summary>
        /// Keywords recognized by the script engine.
        /// </summary>
        private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ScriptConstants.If, ScriptConstants.Else, ScriptConstants.Label, ScriptConstants.Goto, ScriptConstants.Do,
            ScriptConstants.While
        };

        private readonly string _input;
        private int _col = 1;
        private int _line = 1;
        private int _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class for the given script input.
        /// </summary>
        /// <param name="input">The script source text to tokenize.</param>
        public Lexer(string input)
        {
            _input = input;
        }

        /// <summary>
        /// Tokenizes the input script into a list of <see cref="Token"/> objects.
        /// </summary>
        /// <returns>A <see cref="List{Token}"/> containing all tokens found in the script.</returns>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (!IsAtEnd())
            {
                SkipWhitespace();

                var line = _line;
                var col = _col;

                var c = Peek();

                if (c == '\0')
                {
                    Advance(); // skip null chars
                    continue;
                }

                // Identifier or keyword
                if (char.IsLetter(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber)
                {
                    var ident = ReadWhile(ch =>
                        char.IsLetterOrDigit(ch) || ch == '_' ||
                        CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.LetterNumber);

                    var type = Keywords.Contains(ident) ? GetKeywordTokenType(ident) : TokenType.Identifier;

                    tokens.Add(new Token { Type = type, Lexeme = ident, Line = line, Column = col });
                }
                // Number literal
                else if (char.IsDigit(c))
                {
                    var number = ReadWhile(char.IsDigit);
                    tokens.Add(new Token { Type = TokenType.Number, Lexeme = number, Line = line, Column = col });
                }
                // String literal
                else if (c == '"')
                {
                    Advance(); // Skip opening quote
                    var stringBuilder = new StringBuilder();
                    while (!IsAtEnd() && Peek() != '"')
                    {
                        stringBuilder.Append(Peek());
                        Advance();
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
                            Lexeme = "\"" + stringBuilder,
                            Line = line,
                            Column = col
                        });
                    }
                }
                // Operators, punctuation, comments, etc.
                else
                {
                    HandleSingleOrDoubleCharToken(c, tokens, line, col);
                }
            }

            return tokens;
        }

        /// <summary>
        /// Helper to generate a new <see cref="Token"/> with specified type and lexeme.
        /// </summary>
        private Token Token(TokenType type, string lexeme, int line, int col)
        {
            return new Token { Type = type, Lexeme = lexeme, Line = line, Column = col };
        }

        /// <summary>
        /// Skips whitespace characters and updates line/column tracking.
        /// </summary>
        private void SkipWhitespace()
        {
            while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _col = 0;
                    Advance();
                }
                else
                {
                    Advance();
                }
            }
        }

        /// <summary>
        /// Reads consecutive characters while the given predicate is true.
        /// </summary>
        /// <param name="predicate">Function to test each character.</param>
        /// <returns>The substring matching the predicate.</returns>
        private string ReadWhile(Func<char, bool> predicate)
        {
            var start = _pos;
            while (!IsAtEnd() && predicate(Peek()))
            {
                Advance();
            }

            return _input.Substring(start, _pos - start);
        }

        /// <summary>
        /// Returns the character at the current position plus an optional offset.
        /// </summary>
        private char Peek(int offset = 0)
        {
            return _pos + offset < _input.Length ? _input[_pos + offset] : '\0';
        }

        /// <summary>
        /// Advances the current position and column by a given amount.
        /// </summary>
        /// <param name="amount">Number of characters to advance.</param>
        private void Advance(int amount = 1)
        {
            _pos += amount;
            _col += amount;
        }

        /// <summary>
        /// Checks if the lexer has reached the end of the input string.
        /// </summary>
        private bool IsAtEnd()
        {
            return _pos >= _input.Length;
        }

        /// <summary>
        /// Maps a keyword string to the corresponding <see cref="TokenType" />.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns></returns>
        private TokenType GetKeywordTokenType(string keyword)
        {
            return keyword.ToLowerInvariant() switch
            {
                ScriptConstants.If => TokenType.KeywordIf,
                ScriptConstants.Else => TokenType.KeywordElse,
                ScriptConstants.Label => TokenType.Label,
                ScriptConstants.Goto => TokenType.KeywordGoto,
                ScriptConstants.Do => TokenType.KeywordDo,
                ScriptConstants.While => TokenType.KeywordWhile,
                _ => TokenType.Keyword
            };
        }

        /// <summary>
        /// Handles single or double character operators, punctuation, and comments.
        /// </summary>
        private void HandleSingleOrDoubleCharToken(char c, List<Token> tokens, int line, int col)
        {
            switch (c)
            {
                case ';':
                    Advance();
                    tokens.Add(Token(TokenType.Semicolon, ScriptConstants.Semicolon, line, col));
                    break;
                case '.':
                    Advance();
                    tokens.Add(Token(TokenType.Dot, ScriptConstants.Dot, line, col));
                    break;
                case ',':
                    Advance();
                    tokens.Add(Token(TokenType.Comma, ScriptConstants.Comma, line, col));
                    break;
                case '(':
                    Advance();
                    tokens.Add(Token(TokenType.OpenParen, ScriptConstants.OpenParen, line, col));
                    break;
                case ')':
                    Advance();
                    tokens.Add(Token(TokenType.CloseParen, ScriptConstants.CloseParen, line, col));
                    break;
                case '{':
                    Advance();
                    tokens.Add(Token(TokenType.OpenBrace, ScriptConstants.OpenBrace, line, col));
                    break;
                case '}':
                    Advance();
                    tokens.Add(Token(TokenType.CloseBrace, ScriptConstants.CloseBrace, line, col));
                    break;
                case '-':
                    if (Peek(1) == '-')
                    {
                        Advance(2);
                        var comment = ReadWhile(ch => ch != '\n' && ch != '\r');
                        tokens.Add(new Token
                        {
                            Type = TokenType.Comment,
                            Lexeme = comment.Trim(),
                            Line = line,
                            Column = col
                        });
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.Minus, ScriptConstants.Minus, line, col));
                    }

                    break;
                case '+':
                    Advance();
                    tokens.Add(Token(TokenType.Plus, ScriptConstants.Plus, line, col));
                    break;
                case '*':
                    Advance();
                    tokens.Add(Token(TokenType.Star, ScriptConstants.Star, line, col));
                    break;
                case '/':
                    Advance();
                    tokens.Add(Token(TokenType.Slash, ScriptConstants.Slash, line, col));
                    break;
                case '=':
                    if (Peek(1) == '=')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.EqualEqual, ScriptConstants.EqualEqual, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.Equal, ScriptConstants.Equal, line, col));
                    }

                    break;
                case '!':
                    if (Peek(1) == '=')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.BangEqual, ScriptConstants.BangEqual, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.Bang, ScriptConstants.Bang, line, col));
                    }

                    break;
                case '>':
                    if (Peek(1) == '=')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.GreaterEqual, ScriptConstants.GreaterEqual, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.Greater, ScriptConstants.Greater, line, col));
                    }

                    break;
                case '<':
                    if (Peek(1) == '=')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.LessEqual, ScriptConstants.LessEqual, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.Less, ScriptConstants.Less, line, col));
                    }

                    break;
                default:
                    var unknownChar = c;
                    Advance();
                    Console.WriteLine(
                        $"Unknown char: {(int)unknownChar} '{unknownChar}' at Line {line}, Col {col}");
                    tokens.Add(Token(TokenType.Unknown, unknownChar.ToString(), line, col));
                    break;
            }
        }
    }
}