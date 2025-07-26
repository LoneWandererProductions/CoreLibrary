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
        ///     Parses the simple commands returns correct categories and values.
        /// </summary>
        /// <summary>
        ///     Parses the simple commands and returns correct categories and values,
        ///     including explicit If/Else block markers.
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
                Trace.WriteLine($"[{Key}] {Category}: {Value ?? "(null)"}");
            }

            Assert.AreEqual(10, result.Count, "Expected 10 categorized blocks.");

            Assert.IsTrue(result.TryGetCategory(0, out var cat0));
            Assert.AreEqual("Label", cat0, true);
            Assert.IsTrue(result.TryGetValue(0, out var val0));
            Assert.AreEqual("Label(one);", val0.Trim());

            Assert.IsTrue(result.TryGetCategory(1, out var cat1));
            Assert.AreEqual("Command", cat1, true);
            Assert.IsTrue(result.TryGetValue(1, out var val1));
            StringAssert.Contains(val1, "Print");
            StringAssert.Contains(val1, "hello world");

            Assert.IsTrue(result.TryGetCategory(2, out var cat2));
            Assert.AreEqual("Goto", cat2, true);
            Assert.IsTrue(result.TryGetValue(2, out var val2));
            Assert.AreEqual("goto(one);", val2.Trim());

            Assert.IsTrue(result.TryGetCategory(3, out var cat3));
            Assert.AreEqual("If_Condition", cat3, true);
            Assert.IsTrue(result.TryGetValue(3, out var val3));
            Assert.AreEqual("condition", val3.Trim());

            Assert.IsTrue(result.TryGetCategory(4, out var cat4));
            Assert.AreEqual("If_Open", cat4, true);
            Assert.IsTrue(result.TryGetValue(4, out var val4));
            Assert.IsTrue(string.IsNullOrEmpty(val4));

            Assert.IsTrue(result.TryGetCategory(5, out var cat5));
            Assert.AreEqual("Command", cat5, true);
            Assert.IsTrue(result.TryGetValue(5, out var val5));
            StringAssert.Contains(val5, "Print");
            StringAssert.Contains(val5, "if true");

            Assert.IsTrue(result.TryGetCategory(6, out var cat6));
            Assert.AreEqual("If_End", cat6, true);
            Assert.IsTrue(result.TryGetValue(6, out var val6));
            Assert.IsTrue(string.IsNullOrEmpty(val6));

            Assert.IsTrue(result.TryGetCategory(7, out var cat7));
            Assert.AreEqual("Else_Open", cat7, true);
            Assert.IsTrue(result.TryGetValue(7, out var val7));
            Assert.IsTrue(string.IsNullOrEmpty(val7));

            Assert.IsTrue(result.TryGetCategory(8, out var cat8));
            Assert.AreEqual("Command", cat8, true);
            Assert.IsTrue(result.TryGetValue(8, out var val8));
            StringAssert.Contains(val8, "Print");
            StringAssert.Contains(val8, "if false");

            Assert.IsTrue(result.TryGetCategory(9, out var cat9));
            Assert.AreEqual("Else_End", cat9, true);
            Assert.IsTrue(result.TryGetValue(9, out var val9));
            Assert.IsTrue(string.IsNullOrEmpty(val9));
        }

        /// <summary>
        ///     Parses the handles empty input returns empty result.
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
        ///     Parses the handles only comments skips them.
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
        ///     Parses the handles chained method calls as single command.
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
        ///     Parses the handles spacing inside parentheses.
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
