using System;
using System.Collections.Generic;
using System.Diagnostics;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Adjust this if you keep Lexer in another namespace

namespace InterpreteTests
{
    [TestClass]
    public class LexerTests
    {
        /// <summary>
        /// Tokenizes the valid code returns expected tokens.
        /// </summary>
        [TestMethod]
        public void TokenizeValidCodeReturnsExpectedTokens()
        {
            // Arrange
            const string code = @"
        Label(one);
        if(x > 0) {
            com().ext();
        } else {
            -- fallback branch
            fallback();
        }
        Print( hello    world   );
    ";

            var lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                Trace.WriteLine(token);
            }

            // Assert: Base assertions
            Assert.IsTrue(tokens.Count > 0, "Lexer returned no tokens");

            Assert.AreEqual(TokenType.Label, tokens[0].Type);
            Assert.AreEqual("Label", tokens[0].Lexeme, ignoreCase: true);

            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.KeywordIf), "Missing 'if' keyword");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.OpenBrace), "Missing '{'");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Identifier && t.Lexeme.Equals("com", System.StringComparison.OrdinalIgnoreCase)), "Missing 'com' identifier");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Dot), "Missing '.' for method chain");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Comment && t.Lexeme.Contains("fallback")), "Missing comment");

            // 🔹 Assert: Print statement is correctly recognized
            var printToken = tokens.Find(t => t.Type == TokenType.Identifier && t.Lexeme.Equals("Print", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(printToken, "Missing 'Print' identifier");

            var helloIndex = tokens.FindIndex(t => t.Lexeme.Equals("hello", StringComparison.OrdinalIgnoreCase));
            var worldIndex = tokens.FindIndex(t => t.Lexeme.Equals("world", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(helloIndex > 0 && worldIndex > helloIndex, "Expected 'hello' and 'world' tokens inside Print() call");

            // 🔸 Optional: Ensure open/close parens exist around the arguments
            Assert.AreEqual(TokenType.OpenParen, tokens[helloIndex - 1].Type, "Expected '(' before 'hello'");
            Assert.AreEqual(TokenType.CloseParen, tokens[worldIndex + 1].Type, "Expected ')' after 'world'");
        }

    }
}
