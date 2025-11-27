/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine;
 * FILE:        Parser.cs
 * PURPOSE:     Parser for script tokens into ScriptNode tree
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;

namespace Weaver.ScriptEngine
{
    internal sealed class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        /// <summary>
        /// Parses all tokens into a list of ScriptNodes (tree structure for If/DoWhile blocks).
        /// </summary>
        /// <returns>List of ScriptNode</returns>
        public List<ScriptNode> ParseIntoNodes()
        {
            var nodes = new List<ScriptNode>();

            while (!IsAtEnd())
            {
                var token = Peek();
                switch (token.Type)
                {
                    case TokenType.Label:
                        nodes.Add(ParseLabelNode());
                        break;
                    case TokenType.KeywordGoto:
                        nodes.Add(ParseGotoNode());
                        break;
                    case TokenType.Identifier when LookAheadIsAssignment():
                        nodes.Add(ParseAssignmentNode());
                        break;
                    case TokenType.KeywordIf:
                        nodes.Add(ParseIfNode());
                        break;
                    case TokenType.KeywordDo:
                        nodes.Add(ParseDoWhileNode());
                        break;
                    case TokenType.Comment:
                        Advance(); // skip comments
                        break;
                    default:
                        if (token.Type != TokenType.OpenBrace &&
                            token.Type != TokenType.CloseBrace &&
                            token.Type != TokenType.Semicolon)
                        {
                            nodes.Add(ParseCommandNode());
                        }
                        else
                        {
                            Advance(); // skip structural tokens
                        }

                        break;
                }
            }

            return nodes;
        }

        // --- Node Parsers ---

        private LabelNode ParseLabelNode()
        {
            var pos = _position;
            Advance(); // consume 'label'
            var nameToken = Peek();
            if (nameToken.Type != TokenType.Identifier)
                throw new ArgumentException("Expected identifier after 'label'");

            Advance(); // consume identifier
            Match(TokenType.Semicolon);
            return new LabelNode(pos, nameToken.Lexeme);
        }

        private GotoNode ParseGotoNode()
        {
            var pos = _position;
            Advance(); // consume 'goto'
            var targetToken = Peek();
            if (targetToken.Type != TokenType.Identifier)
                throw new ArgumentException("Expected identifier after 'goto'");

            Advance(); // consume identifier
            Match(TokenType.Semicolon);
            return new GotoNode(pos, targetToken.Lexeme);
        }

        private AssignmentNode ParseAssignmentNode()
        {
            var pos = _position;
            var variable = Advance().Lexeme; // consume identifier
            Expect(TokenType.Equal);
            var expr = ReadStatementAsString(); // consume until semicolon
            return new AssignmentNode(pos, variable, expr);
        }

        private CommandNode ParseCommandNode()
        {
            var pos = _position;
            var command = ReadStatementAsString();
            return new CommandNode(pos, command);
        }

        private IfNode ParseIfNode()
        {
            var pos = _position;
            Advance(); // consume 'if'

            var condition = ReadCondition();
            Expect(TokenType.OpenBrace);
            var trueBranch = ParseBlockStatements();

            List<ScriptNode>? falseBranch = null;
            if (!IsAtEnd() && Peek().Type == TokenType.KeywordElse)
            {
                Advance(); // consume 'else'
                Expect(TokenType.OpenBrace);
                falseBranch = ParseBlockStatements();
            }

            return new IfNode(pos, condition, trueBranch, falseBranch);
        }

        private DoWhileNode ParseDoWhileNode()
        {
            var pos = _position;
            Advance(); // consume 'do'
            Expect(TokenType.OpenBrace);
            var body = ParseBlockStatements();

            if (IsAtEnd() || Peek().Type != TokenType.KeywordWhile)
                throw new ArgumentException("Expected 'while' after 'do' block");

            Advance(); // consume 'while'
            var condition = ReadCondition();
            Match(TokenType.Semicolon);

            return new DoWhileNode(pos, body, condition);
        }

        private List<ScriptNode> ParseBlockStatements()
        {
            var statements = new List<ScriptNode>();

            while (!IsAtEnd() && Peek().Type != TokenType.CloseBrace)
            {
                var token = Peek();
                switch (token.Type)
                {
                    case TokenType.KeywordIf:
                        statements.Add(ParseIfNode());
                        break;
                    case TokenType.KeywordDo:
                        statements.Add(ParseDoWhileNode());
                        break;
                    case TokenType.Label:
                        statements.Add(ParseLabelNode());
                        break;
                    case TokenType.KeywordGoto:
                        statements.Add(ParseGotoNode());
                        break;
                    case TokenType.Comment:
                        Advance();
                        break;
                    default:
                        if (token.Type == TokenType.Identifier && LookAheadIsAssignment())
                            statements.Add(ParseAssignmentNode());
                        else if (token.Type != TokenType.OpenBrace &&
                                 token.Type != TokenType.CloseBrace &&
                                 token.Type != TokenType.Semicolon)
                            statements.Add(ParseCommandNode());
                        else
                            Advance();
                        break;
                }
            }

            Expect(TokenType.CloseBrace);
            return statements;
        }

        // --- Helpers ---

        private string ReadStatementAsString()
        {
            var sb = new StringBuilder();
            Token? previous = null;
            var insideParens = false;

            while (!IsAtEnd() && Peek().Type != TokenType.Semicolon)
            {
                var token = Advance();

                if (insideParens && previous != null && IsAlphanumeric(previous.Type) && IsAlphanumeric(token.Type))
                    sb.Append(' ');

                sb.Append(token.Lexeme);

                insideParens = token.Type switch
                {
                    TokenType.OpenParen => true,
                    TokenType.CloseParen => false,
                    _ => insideParens
                };

                previous = token;
            }

            Match(TokenType.Semicolon); // consume semicolon
            return sb.ToString().Trim();
        }

        private string ReadCondition()
        {
            Expect(TokenType.OpenParen);
            var sb = new StringBuilder();
            var depth = 1;

            while (!IsAtEnd() && depth > 0)
            {
                var t = Advance();
                switch (t.Type)
                {
                    case TokenType.OpenParen:
                        depth++;
                        break;
                    case TokenType.CloseParen:
                        depth--;
                        break;
                }

                if (depth > 0) sb.Append(t.Lexeme);
            }

            return sb.ToString().Trim();
        }

        private bool LookAheadIsAssignment() =>
            _position + 1 < _tokens.Count && _tokens[_position + 1].Type == TokenType.Equal;

        private void Expect(TokenType expected)
        {
            if (IsAtEnd() || Peek().Type != expected)
                throw new ArgumentException(
                    $"Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'");

            Advance();
        }

        private bool Match(TokenType type)
        {
            if (IsAtEnd() || Peek().Type != type) return false;

            Advance();
            return true;
        }

        private static bool IsAlphanumeric(TokenType type) =>
            type is TokenType.Identifier or TokenType.Number or TokenType.KeywordIf or TokenType.KeywordElse;

        private Token Peek() => _tokens[_position];
        private Token Advance() => _tokens[_position++];
        private bool IsAtEnd() => _position >= _tokens.Count;
    }
}