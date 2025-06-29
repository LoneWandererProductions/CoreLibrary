using System.Diagnostics;
using System.Linq;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParseSimpleCommands_ReturnsCorrectCategoriesAndValues()
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
            Assert.AreEqual("Command", cat2, ignoreCase: true); // Goto is not treated specially
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

        [TestMethod]
        public void ParseHandlesEmptyInput_ReturnsEmptyResult()
        {
            var lexer = new Lexer("");
            var parser = new Parser(lexer.Tokenize());

            var result = parser.ParseIntoCategorizedBlocks();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseHandlesOnlyComments_SkipsThem()
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
            foreach (var entry in result)
            {
                Trace.WriteLine($"[{entry.Key}] {entry.Category}: {entry.Value}");
            }

            // 🔧 Defensive assertion: ignore empty statements
            Assert.AreEqual(0, result.Count, "Expected no actual parsed statements (only comments)");
        }

        [TestMethod]
        public void ParseHandlesChainedMethodCallsAsSingleCommand()
        {
            var input = @"some().thing().do();";
            var parser = new Parser(new Lexer(input).Tokenize());
            var result = parser.ParseIntoCategorizedBlocks();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Command", result.GetCategory(0), true);
            Assert.AreEqual("some().thing().do();", result[0].Trim());
        }

        [TestMethod]
        public void ParseHandlesSpacingInsideParentheses()
        {
            var input = @"Print(hello   world);";
            var parser = new Parser(new Lexer(input).Tokenize());
            var result = parser.ParseIntoCategorizedBlocks();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Command", result.GetCategory(0), true);
            Assert.AreEqual("Print(hello world);", result[0].Trim()); // note: your parser does space-inside-parens
        }
    }
}
