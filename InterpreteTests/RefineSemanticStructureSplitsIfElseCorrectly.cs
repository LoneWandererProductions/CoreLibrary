/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterpreteTests
 * FILE:        RefineSemanticStructureSplitsIfElseCorrectly.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using ExtendedSystemObjects;
using Interpreter.Extensions;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    [TestClass]
    public class RefineSemanticStructureSplitsIfElseCorrectly
    {
        /// <summary>
        ///     Refines the semantic structure splits if else correctly.
        /// </summary>
        [TestMethod]
        public void RefineSemanticStructureSplitsIfElse()
        {
            const string input = """

        Label(one);
        Print("hello world");
        goto(one);
        if(condition) { Print("if true"); } else { Print("if false"); }
    
""";

            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);

            var rawBlocks = parser.ParseIntoCategorizedBlocks();

            // Semantic refinement pipeline
            var refined = rawBlocks
                .RefineSemanticStructure()
                .RemoveControlStatements()
                .NormalizeJumpTargets();

            foreach (var (Key, Category, Value) in refined)
            {
                Trace.WriteLine($"[{Key}] {Category}: {Value}");
            }

            Assert.AreEqual(10, refined.Count);

            AssertEntry(refined, 0, "Label", "one");
            AssertEntry(refined, 1, "Command", "Print(hello world)");
            AssertEntry(refined, 2, "Goto", "one");

            AssertEntry(refined, 3, "If_Condition", "condition");
            AssertEntry(refined, 4, "If_Open", null);
            AssertEntry(refined, 5, "Command", "Print(if true)");
            AssertEntry(refined, 6, "If_End", null);

            AssertEntry(refined, 7, "Else_Open", null);
            AssertEntry(refined, 8, "Command", "Print(if false)");
            AssertEntry(refined, 9, "Else_End", null);
        }

        /// <summary>
        ///     Verifies that a categorized entry matches expected values.
        /// </summary>
        private static void AssertEntry(CategorizedDictionary<int, string> dict, int key, string expectedCategory,
            string expectedValue)
        {
            var entry = dict.GetCategoryAndValue(key);
            Assert.IsNotNull(entry, $"Expected key {key} to exist.");
            Assert.AreEqual(expectedCategory, entry.Value.Category, true);

            if (expectedValue != null)
            {
                Assert.AreEqual(expectedValue, entry.Value.Value.Trim(), $"Key {key} value mismatch.");
            }
            else
            {
                Assert.IsTrue(string.IsNullOrEmpty(entry.Value.Value), $"Expected null or empty value at key {key}.");
            }
        }
    }
}
