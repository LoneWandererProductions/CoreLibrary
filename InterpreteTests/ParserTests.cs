/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterpreteTests
 * FILE:        ParserTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    [TestClass]
    public class ParserTests
    {
        /// <summary>
        /// Parses the simple commands returns correct categories and values.
        /// </summary>
        [TestMethod]
        public void ParseSimpleCommandsReturnsCorrectCategoriesAndValues()
        {
            var input = @"
                Label(one);
                Print(""hello world"");
                goto(one);
                if(condition) { Print(""if true""); } else { Print(""if false""); }
            ";

            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var result = parser.ParseIntoCategorizedBlocks();


            foreach (var (Key, Category, Value) in result)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            Assert.AreEqual(5, result.Count, "Expected 5 categorized blocks.");

            Assert.IsTrue(result.TryGetCategory(0, out var cat0));
            Assert.AreEqual("Label", cat0, ignoreCase: true);
            Assert.IsTrue(result.TryGetValue(0, out var val0));
            Assert.AreEqual("Label(one);", val0.Trim());

            Assert.IsTrue(result.TryGetCategory(1, out var cat1));
            Assert.AreEqual("Command", cat1, ignoreCase: true);
            Assert.IsTrue(result.TryGetValue(1, out var val1));
            StringAssert.Contains(val1, "Print");
            StringAssert.Contains(val1, "hello world");

            Assert.IsTrue(result.TryGetCategory(2, out var cat2));
            Assert.AreEqual("Goto", cat2, ignoreCase: true); // Goto is not treated specially
            Assert.IsTrue(result.TryGetValue(2, out var val2));
            Assert.AreEqual("goto(one);", val2.Trim());

            Assert.IsTrue(result.TryGetCategory(3, out var cat3));
            Assert.AreEqual("If", cat3, ignoreCase: true);
            Assert.IsTrue(result.TryGetValue(3, out var val3));
            StringAssert.Contains(val3, "if(condition)");
            StringAssert.Contains(val3, "Print");

            Assert.IsTrue(result.TryGetCategory(4, out var cat4));
            Assert.AreEqual("Else", cat4, ignoreCase: true);
            Assert.IsTrue(result.TryGetValue(4, out var val4));
            StringAssert.Contains(val4, "else");
            StringAssert.Contains(val4, "Print");
        }

        /// <summary>
        /// Parses the handles empty input returns empty result.
        /// </summary>
        [TestMethod]
        public void ParseHandlesEmptyInputReturnsEmptyResult()
        {
            var lexer = new Lexer("");
            var parser = new Parser(lexer.Tokenize());

            var result = parser.ParseIntoCategorizedBlocks();

            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        /// Parses the handles only comments skips them.
        /// </summary>
        [TestMethod]
        public void ParseHandlesOnlyCommentsSkipsThem()
        {
            var input = @"
                        -- comment line one
                        -- another comment
                        -- a third one
                    ";

            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var result = parser.ParseIntoCategorizedBlocks();

            // 🔽 Fix: Clean up possible artifacts
            foreach (var (Key, Category, Value) in result)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            // 🔧 Defensive assertion: ignore empty statements
            Assert.AreEqual(0, result.Count, "Expected no actual parsed statements (only comments)");
        }

        /// <summary>
        /// Parses the handles chained method calls as single command.
        /// </summary>
        [TestMethod]
        public void ParseHandlesChainedMethodCallsAsSingleCommand()
        {
            var input = @"some().thing().do();";
            var parser = new Parser(new Lexer(input).Tokenize());
            var result = parser.ParseIntoCategorizedBlocks();

            foreach (var (Key, Category, Value) in result)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Command", result.GetCategory(0), true);
            Assert.AreEqual("some().thing().do();", result[0].Trim());
        }

        /// <summary>
        /// Parses the handles spacing inside parentheses.
        /// </summary>
        [TestMethod]
        public void ParseHandlesSpacingInsideParentheses()
        {
            var input = @"Print(hello   world);";
            var parser = new Parser(new Lexer(input).Tokenize());
            var result = parser.ParseIntoCategorizedBlocks();


            foreach (var (Key, Category, Value) in result)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Command", result.GetCategory(0), true);
            Assert.AreEqual("Print(hello world);", result[0].Trim()); // note: your parser does space-inside-parens
        }
    }
}
