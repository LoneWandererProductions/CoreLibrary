using System;
using System.Diagnostics;
using System.Linq;
using Interpreter.Extensions;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    [TestClass]
    public class RefineSemanticStructureSplitsIfElseCorrectly
    {
        /// <summary>
        /// Refines the semantic structure splits if else correctly.
        /// </summary>
        [TestMethod]
        public void RefineSemanticStructureSplitsIfElse()
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

            var rawBlocks = parser.ParseIntoCategorizedBlocks();

            var refined = rawBlocks.RefineSemanticStructure();

            foreach (var (Key, Category, Value) in refined)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            Assert.AreEqual(6, refined.Count);

            var entry0 = refined.GetCategoryAndValue(0);
            Assert.AreEqual("Label", entry0?.Category);
            Assert.AreEqual("Label(one);", entry0?.Value.Trim());

            var entry1 = refined.GetCategoryAndValue(1);
            Assert.AreEqual("Command", entry1?.Category);
            StringAssert.Contains(entry1?.Value, "Print");
            StringAssert.Contains(entry1?.Value, "hello world");

            var entry2 = refined.GetCategoryAndValue(2);
            Assert.AreEqual("Command", entry2?.Category);
            StringAssert.Contains(entry2?.Value, "goto(one)");

            var entry3 = refined.GetCategoryAndValue(3);
            Assert.AreEqual("If_Condition", entry3?.Category);
            Assert.AreEqual("condition", entry3?.Value.Trim());

            var entry4 = refined.GetCategoryAndValue(4);
            Assert.AreEqual("If_Branch", entry4?.Category);
            StringAssert.Contains(entry4?.Value, "Print");
            StringAssert.Contains(entry4?.Value, "if true");

            var entry5 = refined.GetCategoryAndValue(5);
            Assert.AreEqual("Else_Branch", entry5?.Category);
            StringAssert.Contains(entry5?.Value, "Print");
            StringAssert.Contains(entry5?.Value, "if false");
        }

    }
}
