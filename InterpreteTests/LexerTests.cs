/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterpreteTests
 * FILE:        LexerTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Linq;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// Adjust this if you keep Lexer in another namespace

namespace InterpreteTests
{
    [TestClass]
    public class LexerTests
    {
        /// <summary>
        ///     Tokenizes the valid code returns expected tokens.
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
            var tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                Trace.WriteLine(token);
            }

            // Assert: Base assertions
            Assert.IsTrue(tokens.Count > 0, "Lexer returned no tokens");

            Assert.AreEqual(TokenType.Label, tokens[0].Type);
            Assert.AreEqual("Label", tokens[0].Lexeme, true);

            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.KeywordIf), "Missing 'if' keyword");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.OpenBrace), "Missing '{'");
            Assert.IsTrue(
                tokens.Exists(t =>
                    t.Type == TokenType.Identifier && t.Lexeme.Equals("com", StringComparison.OrdinalIgnoreCase)),
                "Missing 'com' identifier");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Dot), "Missing '.' for method chain");
            Assert.IsTrue(tokens.Exists(t => t.Type == TokenType.Comment && t.Lexeme.Contains("fallback")),
                "Missing comment");

            // 🔹 Assert: Print statement is correctly recognized
            var printToken = tokens.Find(t =>
                t.Type == TokenType.Identifier && t.Lexeme.Equals("Print", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(printToken, "Missing 'Print' identifier");

            var helloIndex = tokens.FindIndex(t => t.Lexeme.Equals("hello", StringComparison.OrdinalIgnoreCase));
            var worldIndex = tokens.FindIndex(t => t.Lexeme.Equals("world", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(helloIndex > 0 && worldIndex > helloIndex,
                "Expected 'hello' and 'world' tokens inside Print() call");

            // 🔸 Optional: Ensure open/close parens exist around the arguments
            Assert.AreEqual(TokenType.OpenParen, tokens[helloIndex - 1].Type, "Expected '(' before 'hello'");
            Assert.AreEqual(TokenType.CloseParen, tokens[worldIndex + 1].Type, "Expected ')' after 'world'");
        }


        /// <summary>
        ///     Lexers the recognizes keywords and identifiers and others.
        /// </summary>
        [TestMethod]
        public void LexerRecognizesKeywordsAndIdentifiersAndOthers()
        {
            const string input = @"
                Label(one);
                Print(""hello world"");
                goto(one);
                if (condition) { Print(""if true""); } else { Print(""if false""); }
                -- this is a comment
                x = 12345;
                y = x >= 100;
                z = x != 50;
                a = b + c - d * e / f;
                unknown_char = '@';
                unicodeId = Привет123;
            ";

            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize();

            // Quick checks for counts of token types
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.KeywordIf), "Missing KeywordIf");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.KeywordElse), "Missing KeywordElse");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Label), "Missing generic Keyword (label)");
            Assert.IsTrue(
                tokens.Any(t =>
                    t.Type == TokenType.Identifier && t.Lexeme.Equals("Print", StringComparison.OrdinalIgnoreCase)),
                "Missing 'Print' identifier");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Number && t.Lexeme == "12345"), "Missing number 12345");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.String && t.Lexeme == "hello world"),
                "Missing string \"hello world\"");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Comment && t.Lexeme.Contains("this is a comment")),
                "Missing comment");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Equal), "Missing '=' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.GreaterEqual), "Missing '>=' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.BangEqual), "Missing '!=' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Plus), "Missing '+' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Minus), "Missing '-' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Star), "Missing '*' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Slash), "Missing '/' token");
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Unknown && t.Lexeme == "@"), "Missing unknown '@' token");

            // Check that unicode identifiers parsed correctly
            Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Identifier && t.Lexeme == "Привет123"),
                "Missing unicode identifier");

            // Spot check a few tokens for expected lexemes and types
            var labelToken = tokens.FirstOrDefault(t =>
                t.Type == TokenType.Label && t.Lexeme.Equals("Label", StringComparison.OrdinalIgnoreCase));
            Assert.IsNotNull(labelToken, "Missing Label keyword token");

            var ifToken = tokens.FirstOrDefault(t => t.Type == TokenType.KeywordIf);
            Assert.IsNotNull(ifToken, "Missing if keyword token");

            var elseToken = tokens.FirstOrDefault(t => t.Type == TokenType.KeywordElse);
            Assert.IsNotNull(elseToken, "Missing else keyword token");

            var stringToken = tokens.FirstOrDefault(t => t.Type == TokenType.String && t.Lexeme == "if true");
            Assert.IsNotNull(stringToken, "Missing string token with 'if true'");

            var commentToken = tokens.FirstOrDefault(t => t.Type == TokenType.Comment);
            Assert.IsNotNull(commentToken, "Missing comment token");

            // Ensure no tokens are null or empty
            Assert.IsFalse(tokens.Any(t => string.IsNullOrWhiteSpace(t.Lexeme)), "Found token with empty lexeme");
        }
    }
}
