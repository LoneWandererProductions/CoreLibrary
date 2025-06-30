/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Parser.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using ExtendedSystemObjects;

namespace Interpreter.ScriptEngine
{
    internal sealed class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public CategorizedDictionary<int, string> ParseIntoCategorizedBlocks()
        {
            var result = new CategorizedDictionary<int, string>();
            var commandIndex = 0;

            while (!IsAtEnd())
            {
                var current = Peek();

                switch (current.Type)
                {
                    case TokenType.KeywordIf:
                        // Parse if statement with condition and block
                        ParseIfStatement(result, ref commandIndex);
                        break;

                    case TokenType.KeywordElse:
                        // Parse else block
                        ParseElseBlock(result, ref commandIndex);
                        break;

                    case TokenType.Label:
                        {
                            var stmt = ReadStatementAsString();
                            result.Add("Label", commandIndex++, stmt);
                            break;
                        }

                    case TokenType.KeywordGoto:
                        {
                            var stmt = ReadStatementAsString();
                            result.Add("Goto", commandIndex++, stmt);
                            break;
                        }

                    case TokenType.Comment:
                        Advance(); // skip comments
                        break;

                    default:
                        {
                            var stmt = ReadStatementAsString();
                            result.Add("Command", commandIndex++, stmt);
                            break;
                        }
                }
            }

            return result;
        }

        private void ParseIfStatement(CategorizedDictionary<int, string> output, ref int commandIndex)
        {
            // Consume 'if' keyword
            Advance();

            // Extract condition between '(' and ')'
            var condition = ReadCondition();
            output.Add("If_Condition", commandIndex++, condition);

            // Expect '{' opening brace of block
            Expect(TokenType.OpenBrace);

            output.Add("If_Open", commandIndex++, null);

            // Parse block statements inside '{ }'
            var statements = ParseBlockStatements();
            foreach (var (cat, stmt) in statements)
            {
                output.Add(cat, commandIndex++, stmt);
            }

            output.Add("If_End", commandIndex++, null);

            // Check if next token is 'else' for else block
            if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
            {
                ParseElseBlock(output, ref commandIndex);
            }
        }

        private void ParseElseBlock(CategorizedDictionary<int, string> output, ref int commandIndex)
        {
            // Consume 'else' keyword
            Advance();

            // Expect '{' opening brace of block
            Expect(TokenType.OpenBrace);

            output.Add("Else_Open", commandIndex++, null);

            // Parse block statements inside '{ }'
            var statements = ParseBlockStatements();
            foreach (var (cat, stmt) in statements)
            {
                output.Add(cat, commandIndex++, stmt);
            }

            output.Add("Else_End", commandIndex++, null);
        }

        private string ReadCondition()
        {
            // Expect '('
            Expect(TokenType.OpenParen);

            var sb = new StringBuilder();
            int parenDepth = 1;

            while (!IsAtEnd() && parenDepth > 0)
            {
                var token = Advance();

                if (token.Type == TokenType.OpenParen)
                    parenDepth++;
                else if (token.Type == TokenType.CloseParen)
                    parenDepth--;

                if (parenDepth > 0)
                    sb.Append(token.Lexeme);
            }

            return sb.ToString().Trim();
        }

        private List<(string Category, string? Statement)> ParseBlockStatements()
        {
            var statements = new List<(string, string?)>();

            while (!IsAtEnd() && Peek().Type != TokenType.CloseBrace)
            {
                var token = Peek();

                switch (token.Type)
                {
                    case TokenType.KeywordIf:
                        // Handle nested if
                        Advance(); // consume 'if'
                        var condition = ReadCondition();
                        statements.Add(("If_Condition", condition));
                        Expect(TokenType.OpenBrace);
                        statements.Add(("If_Open", null));
                        var ifBody = ParseBlockStatements();
                        statements.AddRange(ifBody);
                        statements.Add(("If_End", null));

                        // Optional else
                        if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
                        {
                            Advance(); // consume 'else'
                            Expect(TokenType.OpenBrace);
                            statements.Add(("Else_Open", null));
                            var elseBody = ParseBlockStatements();
                            statements.AddRange(elseBody);
                            statements.Add(("Else_End", null));
                        }

                        break;

                    case TokenType.Label:
                        statements.Add(("Label", ReadStatementAsString()));
                        break;

                    case TokenType.KeywordGoto:
                        statements.Add(("Goto", ReadStatementAsString()));
                        break;

                    case TokenType.Comment:
                        Advance(); // skip
                        break;

                    default:
                        statements.Add(("Command", ReadStatementAsString()));
                        break;
                }
            }

            Expect(TokenType.CloseBrace); // consume '}'
            return statements;
        }


        private void Expect(TokenType expected)
        {
            if (IsAtEnd() || Peek().Type != expected)
            {
                throw new ArgumentException($"Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'.");
            }

            Advance();
        }


        private string ReadStatementAsString()
        {
            var sb = new StringBuilder();
            var insideParens = false;
            Token? previous = null;

            while (!IsAtEnd() && Peek().Type != TokenType.Semicolon)
            {
                var current = Advance();

                if (insideParens &&
                    previous != null &&
                    IsAlphanumeric(previous.Type) &&
                    IsAlphanumeric(current.Type))
                {
                    sb.Append(' ');
                }

                sb.Append(current.Lexeme);

                if (current.Type == TokenType.OpenParen)
                {
                    insideParens = true;
                }
                else if (current.Type == TokenType.CloseParen)
                {
                    insideParens = false;
                }

                previous = current;
            }

            if (Match(TokenType.Semicolon))
            {
                sb.Append(';');
            }

            return sb.ToString();
        }

        private bool IsAlphanumeric(TokenType type)
        {
            return type == TokenType.Identifier ||
                   type == TokenType.KeywordIf ||
                   type == TokenType.KeywordElse ||
                   type == TokenType.Number; // add any others you want spaced
        }

        private bool IsAtEnd()
        {
            return _position >= _tokens.Count;
        }

        private Token Advance()
        {
            return _tokens[_position++];
        }

        private Token Peek()
        {
            return _tokens[_position];
        }

        private bool Match(TokenType type)
        {
            if (IsAtEnd() || _tokens[_position].Type != type)
            {
                return false;
            }

            _position++;
            return true;
        }
    }
}
