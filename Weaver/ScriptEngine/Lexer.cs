/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lexer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// A lexical analyzer that splits a script into its smallest meaningful components (tokens).
    /// Supports identifiers, keywords, numbers, strings, operators, and comments.
    /// </summary>
    public sealed class Lexer
    {
        /// <summary>
        /// Keywords recognized by the script engine.
        /// </summary>
        private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ScriptConstants.If, ScriptConstants.Else, ScriptConstants.Label, ScriptConstants.Goto, ScriptConstants.Do,
            ScriptConstants.While
        };

        /// <summary>
        /// The input
        /// </summary>
        private readonly string _input;

        /// <summary>
        /// The col
        /// </summary>
        private int _col = 1;

        /// <summary>
        /// The line
        /// </summary>
        private int _line = 1;

        /// <summary>
        /// The position
        /// </summary>
        private int _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class for the given script input.
        /// </summary>
        /// <param name="input">The script source text to tokenize.</param>
        public Lexer(string input)
        {
            // normalize line endings
            _input = input.Replace("\r\n", "\n");
        }

        /// <summary>
        /// Static version: Tokenizes the input script and returns only the lexemes as a List of strings.
        /// </summary>
        /// <param name="input">The script source text to tokenize.</param>
        /// <returns>List of lexemes in order of appearance.</returns>
        public static List<string> Tokenize(string input)
        {
            var lexer = new Lexer(input);
            // Rufe die private Instanzmethode auf, die List<Token> zurückgibt
            return lexer.Tokenize()
                .Select(t => t.Lexeme!)
                .ToList();
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

                if (IsAtEnd())
                    break;

                // Check for inline comment
                if (Peek() == '/' && Peek(1) == '/')
                {
                    // Skip to end of line
                    while (!IsAtEnd() && Peek() != '\n')
                        Advance();
                    continue;
                }

                var c = Peek();

                // Numbers (integer or float)
                if (char.IsDigit(c))
                {
                    var start = _pos;
                    var hasDot = false;

                    while (!IsAtEnd() && (char.IsDigit(Peek()) || (!hasDot && Peek() == '.')))
                    {
                        if (Peek() == '.')
                            hasDot = true;
                        Advance();
                    }

                    var number = _input.Substring(start, _pos - start);
                    tokens.Add(new Token { Type = TokenType.Number, Lexeme = number, Line = line, Column = col });
                    continue;
                }

                // Identifiers / keywords
                if (char.IsLetter(c) || c == '_')
                {
                    var ident = ReadWhile(ch => char.IsLetterOrDigit(ch) || ch == '_');

                    TokenType type;

                    // Convert logical keywords to operator tokens
                    switch (ident.ToLowerInvariant())
                    {
                        case "and":
                            type = TokenType.LogicalAnd;
                            ident = "&&";
                            break;
                        case "or":
                            type = TokenType.LogicalOr;
                            ident = "||";
                            break;
                        case "not":
                            type = TokenType.Bang;
                            ident = "!";
                            break;
                        default:
                            type = Keywords.Contains(ident) ? GetKeywordTokenType(ident) : TokenType.Identifier;
                            break;
                    }

                    tokens.Add(new Token { Type = type, Lexeme = ident, Line = line, Column = col });
                    continue;
                }


                // Strings
                if (c == '"')
                {
                    Advance(); // skip opening "
                    var sb = new StringBuilder();
                    while (!IsAtEnd() && Peek() != '"')
                    {
                        sb.Append(Peek());
                        Advance();
                    }

                    if (!IsAtEnd()) Advance(); // skip closing "
                    tokens.Add(new Token
                        { Type = TokenType.String, Lexeme = sb.ToString(), Line = line, Column = col });
                    continue;
                }

                // Try multi-char operators first
                if (TryHandleDoubleCharOperator(tokens, line, col))
                    continue;

                // Operators, braces, punctuation
                HandleSingleOrDoubleCharToken(c, tokens, line, col);
            }

            return tokens;
        }

        /// <summary>
        /// Helper to generate a new <see cref="Token" /> with specified type and lexeme.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="line">The line.</param>
        /// <param name="col">The col.</param>
        /// <returns>The token.</returns>
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
        /// <returns>Type of the token.</returns>
        private static TokenType GetKeywordTokenType(string keyword) => keyword.ToLowerInvariant() switch
        {
            ScriptConstants.If => TokenType.KeywordIf,
            ScriptConstants.Else => TokenType.KeywordElse,
            ScriptConstants.Label => TokenType.Label,
            ScriptConstants.Goto => TokenType.KeywordGoto,
            ScriptConstants.Do => TokenType.KeywordDo,
            ScriptConstants.While => TokenType.KeywordWhile,
            _ => TokenType.Keyword
        };

        /// <summary>
        /// Tries the handle double character operator.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="line">The line.</param>
        /// <param name="col">The col.</param>
        /// <returns>Convert operators to Enum Token.</returns>
        private bool TryHandleDoubleCharOperator(List<Token> tokens, int line, int col)
        {
            if (_pos + 1 >= _input.Length)
                return false;

            var two = _input.Substring(_pos, 2);
            TokenType? type = two switch
            {
                "==" => TokenType.EqualEqual,
                "!=" => TokenType.BangEqual,
                ">=" => TokenType.GreaterEqual,
                "<=" => TokenType.LessEqual,
                "&&" => TokenType.LogicalAnd,
                "||" => TokenType.LogicalOr,
                _ => null
            };

            if (type != null)
            {
                tokens.Add(new Token
                {
                    Type = type.Value,
                    Lexeme = two,
                    Line = line,
                    Column = col
                });
                Advance(2);
                return true;
            }

            return false;
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
                case '&':
                    if (Peek(1) == '&')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.LogicalAnd, ScriptConstants.LogicalAndSymbol, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.BitAnd, "&", line, col));
                    }

                    break;

                case '|':
                    if (Peek(1) == '|')
                    {
                        Advance(2);
                        tokens.Add(Token(TokenType.LogicalOr, ScriptConstants.LogicalOrSymbol, line, col));
                    }
                    else
                    {
                        Advance();
                        tokens.Add(Token(TokenType.BitOr, "|", line, col));
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