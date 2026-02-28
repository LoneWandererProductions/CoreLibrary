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
    /// <summary>
    /// parser for script tokens into ScriptNode tree
    /// </summary>
    internal sealed class Parser
    {
        /// <summary>
        /// The tokens
        /// </summary>
        private readonly List<Token> _tokens;

        /// <summary>
        /// The position
        /// </summary>
        private int _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
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

        /// <summary>
        /// Parses the label node.
        /// </summary>
        /// <returns>Label Node record.</returns>
        /// <exception cref="System.ArgumentException">Expected identifier after 'label'</exception>
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

        /// <summary>
        /// Parses the goto node.
        /// </summary>
        /// <returns>Goto Node record.</returns>
        /// <exception cref="System.ArgumentException">Expected identifier after 'goto'</exception>
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

        /// <summary>
        /// Parses the assignment node.
        /// </summary>
        /// <returns>Assignment Node record.</returns>
        private AssignmentNode ParseAssignmentNode()
        {
            var pos = _position;
            var variable = Advance().Lexeme; // consume identifier
            Expect(TokenType.Equal);
            var expr = ReadStatementAsString(); // consume until semicolon
            return new AssignmentNode(pos, variable, expr);
        }

        /// <summary>
        /// Parses the command node.
        /// </summary>
        /// <returns>Command Node record.</returns>
        private CommandNode ParseCommandNode()
        {
            var pos = _position;
            var command = ReadStatementAsString();
            return new CommandNode(pos, command);
        }

        /// <summary>
        /// Parses if node.
        /// </summary>
        /// <returns>If Node record.</returns>
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

        /// <summary>
        /// Parses the do while node.
        /// </summary>
        /// <returns>Do7While Node record.</returns>
        /// <exception cref="System.ArgumentException">Expected 'while' after 'do' block</exception>
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

        /// <summary>
        /// Parses the block statements.
        /// </summary>
        /// <returns>The complete Script Node.</returns>
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

        /// <summary>
        /// Reads the statement as string.
        /// </summary>
        /// <returns>Statement as string</returns>
        private string ReadStatementAsString()
        {
            var sb = new StringBuilder();
            Token? previous = null;
            var insideParens = false;

            //while (!IsAtEnd() && Peek().Type != TokenType.Semicolon)
            while (!IsAtEnd() &&
                   Peek().Type != TokenType.Semicolon &&
                   Peek().Type != TokenType.CloseBrace)
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

        /// <summary>
        /// Reads the condition.
        /// </summary>
        /// <returns>Condition as string</returns>
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

        /// <summary>
        /// Looks the ahead is assignment.
        /// </summary>
        /// <returns></returns>
        private bool LookAheadIsAssignment() =>
            _position + 1 < _tokens.Count && _tokens[_position + 1].Type == TokenType.Equal;

        /// <summary>
        /// Expects the specified expected.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <exception cref="System.ArgumentException">Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'</exception>
        private void Expect(TokenType expected)
        {
            if (IsAtEnd() || Peek().Type != expected)
                throw new ArgumentException(
                    $"Expected token '{expected}' but found '{(IsAtEnd() ? "EOF" : Peek().Type.ToString())}'");

            Advance();
        }

        /// <summary>
        /// Matches the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private bool Match(TokenType type)
        {
            if (IsAtEnd() || Peek().Type != type) return false;

            Advance();
            return true;
        }

        /// <summary>
        /// Determines whether the specified type is alphanumeric.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is alphanumeric; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAlphanumeric(TokenType type) =>
            type is TokenType.Identifier or TokenType.Number or TokenType.KeywordIf or TokenType.KeywordElse;

        /// <summary>
        /// Peeks this instance.
        /// </summary>
        /// <returns>Token at current position</returns>
        private Token Peek() => _tokens[_position];

        /// <summary>
        /// Advances this instance.
        /// </summary>
        /// <returns>Token at next position.</returns>
        private Token Advance() => _tokens[_position++];

        /// <summary>
        /// Determines whether [is at end].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is at end]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAtEnd() => _position >= _tokens.Count;
    }
}