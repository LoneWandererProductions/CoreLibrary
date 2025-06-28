using System.Collections.Generic;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Adjust this if you keep Lexer in another namespace

namespace InterpreteTests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Tokenize_ValidCode_ReturnsExpectedTokens()
        {
            // Arrange
            const string code = @"
                Label(one);
                if(x > 0) {
                    com().ext();
                } else {
                    -- fallback branch
                    fallback();
                }";

            var lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
            Assert.IsTrue(tokens.Count > 0, "Lexer returned no tokens");

            Assert.AreEqual(TokenType.Label, tokens[0].Type);
            Assert.AreEqual("Label", tokens[0].Lexeme, ignoreCase: true);

            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.KeywordIf), "Missing 'if' keyword");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.OpenBrace), "Missing '{'");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Identifier && t.Lexeme.Equals("com", System.StringComparison.OrdinalIgnoreCase)), "Missing 'com' identifier");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Dot), "Missing '.' for method chain");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Comment && t.Lexeme.Contains("fallback")), "Missing comment");
        }
    }
}
