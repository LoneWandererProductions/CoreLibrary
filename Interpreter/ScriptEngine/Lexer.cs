/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Lexer.cs
 * PURPOSE:     Split our script into their smallest parts
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Interpreter.ScriptEngine;

internal sealed class Lexer
{
    /// <summary>
    ///     The keywords that are used right now
    /// </summary>
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "if", "else", "label", "goto"
    };

    private readonly string _input;
    private int _col = 1;
    private int _line = 1;
    private int _pos;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Lexer" /> class.
    /// </summary>
    /// <param name="input">The input.</param>
    public Lexer(string input)
    {
        _input = input;
    }

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


            if (char.IsLetter(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LetterNumber)
            {
                var ident = ReadWhile(ch =>
                    char.IsLetterOrDigit(ch) || ch == '_' ||
                    CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.LetterNumber);
                if (Keywords.Contains(ident))
                {
                    var type = ident.Equals("if", StringComparison.OrdinalIgnoreCase)
                        ? TokenType.KeywordIf
                        : ident.Equals("else", StringComparison.OrdinalIgnoreCase)
                            ? TokenType.KeywordElse
                            : ident.Equals("label", StringComparison.OrdinalIgnoreCase)
                                ? TokenType.Label
                                : // or TokenType.KeywordLabel
                                ident.Equals("goto", StringComparison.OrdinalIgnoreCase)
                                    ? TokenType.KeywordGoto
                                    : // add this token type
                                    TokenType.Keyword; // fallback

                    tokens.Add(new Token { Type = type, Lexeme = ident, Line = line, Column = col });
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
            else
            {
                switch (c)
                {
                    case ';':
                        Advance();
                        tokens.Add(Token(TokenType.Semicolon, ";", line, col));
                        break;
                    case '.':
                        Advance();
                        tokens.Add(Token(TokenType.Dot, ".", line, col));
                        break;
                    case ',':
                        Advance();
                        tokens.Add(Token(TokenType.Comma, ",", line, col));
                        break;
                    case '(':
                        Advance();
                        tokens.Add(Token(TokenType.OpenParen, "(", line, col));
                        break;
                    case ')':
                        Advance();
                        tokens.Add(Token(TokenType.CloseParen, ")", line, col));
                        break;
                    case '{':
                        Advance();
                        tokens.Add(Token(TokenType.OpenBrace, "{", line, col));
                        break;
                    case '}':
                        Advance();
                        tokens.Add(Token(TokenType.CloseBrace, "}", line, col));
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
                            tokens.Add(Token(TokenType.Minus, "-", line, col));
                        }

                        break;
                    case '+':
                        Advance();
                        tokens.Add(Token(TokenType.Plus, "+", line, col));
                        break;
                    case '*':
                        Advance();
                        tokens.Add(Token(TokenType.Star, "*", line, col));
                        break;
                    case '/':
                        Advance();
                        tokens.Add(Token(TokenType.Slash, "/", line, col));
                        break;
                    case '=':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            tokens.Add(Token(TokenType.EqualEqual, "==", line, col));
                        }
                        else
                        {
                            Advance();
                            tokens.Add(Token(TokenType.Equal, "=", line, col));
                        }

                        break;
                    case '!':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            tokens.Add(Token(TokenType.BangEqual, "!=", line, col));
                        }
                        else
                        {
                            Advance();
                            tokens.Add(Token(TokenType.Bang, "!", line, col));
                        }

                        break;
                    case '>':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            tokens.Add(Token(TokenType.GreaterEqual, ">=", line, col));
                        }
                        else
                        {
                            Advance();
                            tokens.Add(Token(TokenType.Greater, ">", line, col));
                        }

                        break;
                    case '<':
                        if (Peek(1) == '=')
                        {
                            Advance(2);
                            tokens.Add(Token(TokenType.LessEqual, "<=", line, col));
                        }
                        else
                        {
                            Advance();
                            tokens.Add(Token(TokenType.Less, "<", line, col));
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

        return tokens;
    }

    private Token Token(TokenType type, string lexeme, int line, int col)
    {
        return new Token { Type = type, Lexeme = lexeme, Line = line, Column = col };
    }

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
            else if (Peek() == '\r')
            {
                Advance();
            }
            else
            {
                Advance();
            }
        }
    }

    private string ReadWhile(Func<char, bool> predicate)
    {
        var start = _pos;
        while (!IsAtEnd() && predicate(Peek()))
        {
            Advance();
        }

        return _input.Substring(start, _pos - start);
    }

    private char Peek(int offset = 0)
    {
        return _pos + offset < _input.Length ? _input[_pos + offset] : '\0';
    }

    private void Advance(int amount = 1)
    {
        _pos += amount;
        _col += amount;
    }

    private bool IsAtEnd()
    {
        return _pos >= _input.Length;
    }
}
