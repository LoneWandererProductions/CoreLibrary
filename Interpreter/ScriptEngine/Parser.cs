#nullable enable
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
            var builder = new StringBuilder();

            while (!IsAtEnd())
            {
                var current = Peek();

                string currentCategory;

                switch (current.Type)
                {
                    case TokenType.KeywordIf:
                        currentCategory = "If";
                        builder.Clear();
                        builder.Append(ReadBlockAsString());
                        result.Add(currentCategory, commandIndex++, builder.ToString());
                        break;

                    case TokenType.KeywordElse:
                        currentCategory = "Else";
                        builder.Clear();
                        builder.Append(ReadBlockAsString());
                        result.Add(currentCategory, commandIndex++, builder.ToString());
                        break;

                    case TokenType.Label:
                        currentCategory = "Label";
                        builder.Clear();
                        builder.Append(ReadStatementAsString());
                        result.Add(currentCategory, commandIndex++, builder.ToString());
                        break;

                    case TokenType.Comment:
                        Advance(); // skip comments
                        break;

                    default:
                        builder.Clear();
                        builder.Append(ReadStatementAsString());
                        result.Add("Command", commandIndex++, builder.ToString());
                        break;
                }
            }

            return result;
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

        private string ReadBlockAsString()
        {
            var sb = new StringBuilder();
            var braceCount = 0;

            while (!IsAtEnd())
            {
                var token = Advance();
                sb.Append(token.Lexeme);

                if (token.Type == TokenType.OpenBrace)
                {
                    braceCount++;
                }
                else if (token.Type == TokenType.CloseBrace)
                {
                    braceCount--;
                    if (braceCount == 0)
                    {
                        break;
                    }
                }
            }

            return sb.ToString();
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
