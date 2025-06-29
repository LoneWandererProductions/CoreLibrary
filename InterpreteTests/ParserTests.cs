using System.Diagnostics;
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
            // Arrange: simple input with labels, commands, goto
            var input = @"
                Label(one);
                Print(""hello world"");
                goto(one);
                if(condition) { Print(""if true""); } else { Print(""if false""); }
            ";

            var lex = new Lexer(input);
            var tokens = lex.Tokenize();

            foreach (var token in tokens)
            {
                Trace.WriteLine("Token:");
                Trace.WriteLine(token);
            }

            // Act: parse input - assuming you have a Parse method that returns CategorizedDictionary<int, string>
            var parser = new Parser(tokens); // replace with your actual parser class
            var result = parser.ParseIntoCategorizedBlocks();

            foreach (var part in result)
            {
                Trace.WriteLine("Parsed:");
                Trace.WriteLine(part);
            }

            // Assert: verify key, category, value correctness
            Assert.IsTrue(result.TryGetCategory(0, out var cat0));
            Assert.AreEqual("LABEL", cat0, true);
            Assert.IsTrue(result.TryGetValue(0, out var val0));
            Assert.AreEqual("Label(one);", val0.Trim());

            Assert.IsTrue(result.TryGetCategory(1, out var cat1));
            Assert.AreEqual("COMMAND", cat1, true);
            Assert.IsTrue(result.TryGetValue(1, out var val1));
            //Assert.AreEqual("Print(hello world);", val1.Trim());

            Assert.IsTrue(result.TryGetCategory(2, out var cat2));
            //Assert.AreEqual("GOTO", cat2, ignoreCase: true);
            Assert.IsTrue(result.TryGetValue(2, out var val2));
            Assert.AreEqual("goto(one);", val2.Trim());

            Assert.IsTrue(result.TryGetCategory(3, out var cat3));
            //Assert.IsTrue(cat3.StartsWith("IF")); // could be IF_1 or similar depending on your layered category scheme
            Assert.IsTrue(result.TryGetValue(3, out var val3));
            StringAssert.Contains(val3, "if(condition)");

            Assert.IsTrue(result.TryGetCategory(4, out var cat4));
//            Assert.IsTrue(cat4.StartsWith("ELSE"));
            Assert.IsTrue(result.TryGetValue(4, out var val4));
            StringAssert.Contains(val4, "else");
        }
    }
}
