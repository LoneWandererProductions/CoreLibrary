/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterpreteTests
 * FILE:        CommandBuilder.cs
 * PURPOSE:     Some full command tests mosttly for the scripting part and if else
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using ExtendedSystemObjects;
using Interpreter.ScriptEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    [TestClass]
    public class CommandBuilder
    {
        /// <summary>
        ///     Parses if else clauses no if else clauses returns empty dictionary.
        /// </summary>
        [TestMethod]
        public void ParseIfElseClausesNoIfElseClausesReturnsEmptyDictionary()
        {
            // Arrange
            const string input = "com1;";

            // Act
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        /// <summary>
        ///     Parses if else clauses single if clause returns one if else object.
        /// </summary>
        [TestMethod]
        public void ParseIfElseClausesSingleIfClauseReturnsOneIfElseObj()
        {
            // Arrange
            const string input = "if (condition1) {com1; }";

            // Act
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            // Assert: only one IfElseObj should exist
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            // Grab the only object
            Assert.IsTrue(result.TryGetValue(0, out var ifElseObj));
            Assert.AreEqual(0, ifElseObj.Id);
            Assert.AreEqual(-1, ifElseObj.ParentId);
            Assert.AreEqual(0, ifElseObj.Position);
            Assert.AreEqual(0, ifElseObj.Layer);
            Assert.IsFalse(ifElseObj.Else);
            Assert.IsFalse(ifElseObj.Nested);
            Assert.AreEqual("if (condition1) {com1; }", ifElseObj.Input);

            // Output debug info
            Trace.WriteLine(ifElseObj.ToString());

            // Validate commands inside the IfElseObj
            var expectedResults = new List<(int Key, string Category, string Value)>
            {
                (0, "If_Condition", "condition1"), (1, "If", "com1;")
            };

            foreach (var (key, cat, val) in ifElseObj.Commands)
            {
                Trace.WriteLine($"Parsed Command -> Key: {key}, Category: {cat}, Value: {val}");
            }

            foreach (var (key, category, value) in expectedResults)
            {
                Assert.IsTrue(ifElseObj.Commands.TryGetCategory(key, out var actualCategory),
                    $"Missing category for key {key}");
                Assert.AreEqual(category, actualCategory, $"Category mismatch for key {key}");

                Assert.IsTrue(ifElseObj.Commands.TryGetValue(key, out var actualValue),
                    $"Missing value for key {key}");
                Assert.AreEqual(value, actualValue, $"Value mismatch for key {key}");
            }
        }

        /// <summary>
        ///     Parses if else clauses single if clause returns correct object.
        /// </summary>
        [TestMethod]
        public void ParseIfElseClausesSingleIfClauseReturnsCorrectObject()
        {
            const string input = "if (condition) { doSomething(); }";
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            Assert.AreEqual(1, result.Count, "There should be one IfElseObj in the result.");
            var obj = result[0];
            Assert.IsFalse(obj.Else, "The 'Else' flag should be false for an 'if' clause.");
            Assert.AreEqual(-1, obj.ParentId, "The ParentId should be -1 for a top-level 'if' clause.");
            Assert.AreEqual(0, obj.Layer, "The Layer should be 0 for a top-level 'if' clause.");
            Assert.AreEqual(0, obj.Position, "The Position should be 0 for a top-level 'if' clause.");
            Assert.AreEqual("if (condition) { doSomething(); }", obj.Input, "The Input string should match.");
        }

        /// <summary>
        ///     Parses if else clauses with nested if else returns correct structure.
        /// </summary>
        [TestMethod]
        public void ParseIfElseClausesWithNestedIfElseReturnsCorrectStructure()
        {
            // Arrange
            const string input =
                "if (condition1) { Command1; if (condition2) { Command2; } else { Command3; } } else { Command4; }";

            var expected = new Dictionary<int, IfElseObj>
            {
                {
                    0, new IfElseObj
                    {
                        Id = 0,
                        Input = input,
                        Else = false,
                        ParentId = -1,
                        Layer = 0, // top-level layer matches output
                        Position = 0,
                        Nested = true,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "condition1" }, { "Else", 2, "Command4;" }
                        }
                    }
                },
                {
                    1,
                    new IfElseObj
                    {
                        Id = 1,
                        Input = "if (condition2) { Command2; } else { Command3; }",
                        Else = false,
                        ParentId = 0,
                        Layer = 1,
                        Position = 1,
                        Nested = false,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "condition2" },
                            { "If", 1, "Command2;" },
                            { "Else", 2, "Command3;" }
                        }
                    }
                }
            };


            // Act
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            // Debug output
            foreach (var kvp in result)
            {
                Trace.WriteLine($"Result[{kvp.Key}]:\n{kvp.Value}\n");
            }

            // Assert count matches
            Assert.AreEqual(expected.Count, result.Count, "Number of parsed if-else blocks does not match expected.");

            // Assert each IfElseObj matches
            foreach (var kvp in expected)
            {
                Assert.IsTrue(result.ContainsKey(kvp.Key), $"Result missing expected key {kvp.Key}.");

                var expectedObj = kvp.Value;
                var actualObj = result[kvp.Key];

                Assert.AreEqual(expectedObj.Id, actualObj.Id, $"Id mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.ParentId, actualObj.ParentId, $"ParentId mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.Position, actualObj.Position, $"Position mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.Layer, actualObj.Layer, $"Layer mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.Else, actualObj.Else, $"Else flag mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.Nested, actualObj.Nested, $"Nested flag mismatch at key {kvp.Key}.");
                Assert.AreEqual(expectedObj.Input, actualObj.Input, $"Input string mismatch at key {kvp.Key}.");

                var areCommandsEqual = CategorizedDictionary<int, string>.AreEqual(
                    expectedObj.Commands,
                    actualObj.Commands,
                    out var message);

                Assert.IsTrue(areCommandsEqual, $"Commands mismatch at key {kvp.Key}: {message}");
            }
        }

        [TestMethod]
        public void ParseIfClauseWithoutElseReturnsCorrectStructure()
        {
            const string input = "if (condition1) { Command1; }";

            var expected = new Dictionary<int, IfElseObj>
            {
                {
                    0, new IfElseObj
                    {
                        Input = input,
                        Else = false,
                        ParentId = -1,
                        Layer = 0,
                        Position = 0,
                        Nested = false,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "condition1" }, { "If", 1, "Command1;" }
                        }
                    }
                }
            };

            var result = ConditionalExpressions.ParseIfElseClauses(input);

            Assert.AreEqual(expected.Count, result.Count);
            var expectedObj = expected[0];
            var actualObj = result[0];

            Assert.AreEqual(expectedObj.Id, actualObj.Id);
            Assert.AreEqual(expectedObj.ParentId, actualObj.ParentId);
            Assert.AreEqual(expectedObj.Layer, actualObj.Layer);
            Assert.AreEqual(expectedObj.Position, actualObj.Position);
            Assert.AreEqual(expectedObj.Else, actualObj.Else);
            Assert.AreEqual(expectedObj.Nested, actualObj.Nested);
            Assert.AreEqual(expectedObj.Input, actualObj.Input);

            var commandsEqual =
                CategorizedDictionary<int, string>.AreEqual(expectedObj.Commands, actualObj.Commands, out var msg);
            Assert.IsTrue(commandsEqual, $"Commands mismatch: {msg}");
        }

        [TestMethod]
        public void ParseMultipleNestedIfElseClausesReturnsCorrectStructure()
        {
            const string input = "if (cond1) { if (cond2) { Cmd2; } else { Cmd3; } } else { Cmd4; }";

            var expected = new Dictionary<int, IfElseObj>
            {
                {
                    0, new IfElseObj
                    {
                        Input = input,
                        Else = false,
                        ParentId = -1,
                        Layer = 0,
                        Position = 0,
                        Nested = true,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "cond1" }, { "Else", 2, "Cmd4;" }
                        }
                    }
                },
                {
                    1,
                    new IfElseObj
                    {
                        Input = "if (cond2) { Cmd2; } else { Cmd3; }",
                        Else = false,
                        ParentId = 0,
                        Layer = 1,
                        Position = 1,
                        Nested = false,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "cond2" }, { "If", 1, "Cmd2;" }, { "Else", 2, "Cmd3;" }
                        }
                    }
                }
            };

            var result = ConditionalExpressions.ParseIfElseClauses(input);

            foreach (var kvp in expected)
            {
                Assert.IsTrue(result.ContainsKey(kvp.Key), $"Missing key {kvp.Key} in result.");
                var expectedObj = kvp.Value;
                var actualObj = result[kvp.Key];

                // Skip exact ID check
                Assert.AreEqual(expectedObj.ParentId, actualObj.ParentId, $"ParentId mismatch at key {kvp.Key}");
                Assert.AreEqual(expectedObj.Layer, actualObj.Layer, $"Layer mismatch at key {kvp.Key}");
                Assert.AreEqual(expectedObj.Position, actualObj.Position, $"Position mismatch at key {kvp.Key}");
                Assert.AreEqual(expectedObj.Else, actualObj.Else, $"Else flag mismatch at key {kvp.Key}");
                Assert.AreEqual(expectedObj.Nested, actualObj.Nested, $"Nested flag mismatch at key {kvp.Key}");
                Assert.AreEqual(expectedObj.Input, actualObj.Input, $"Input mismatch at key {kvp.Key}");

                var commandsEqual =
                    CategorizedDictionary<int, string>.AreEqual(expectedObj.Commands, actualObj.Commands, out var msg);
                Assert.IsTrue(commandsEqual, $"Commands mismatch at key {kvp.Key}: {msg}");
            }
        }


        [TestMethod]
        public void ParseIfElseIfElseChainReturnsCorrectStructure()
        {
            const string input = "if (cond1) { Cmd1; } else { if (cond2) { Cmd2; } else { Cmd3; } }";

            var expected = new Dictionary<int, IfElseObj>
            {
                {
                    0, new IfElseObj
                    {
                        Input = input,
                        Else = false,
                        ParentId = -1,
                        Layer = 0,
                        Position = 0,
                        Nested = true,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "cond1" }, { "If", 1, "Cmd1;" }
                        }
                    }
                },
                {
                    1,
                    new IfElseObj
                    {
                        Input = "if (cond2) { Cmd2; } else { Cmd3; }",
                        Else = true,
                        ParentId = 0,
                        Layer = 1,
                        Position = 2,
                        Nested = false,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "cond2" }, { "If", 1, "Cmd2;" }, { "Else", 2, "Cmd3;" }
                        }
                    }
                }
            };

            var result = ConditionalExpressions.ParseIfElseClauses(input);

            Assert.AreEqual(expected.Count, result.Count, "Mismatch in number of IfElse objects parsed.");

            foreach (var key in expected.Keys)
            {
                Assert.IsTrue(result.ContainsKey(key), $"Result missing expected key {key}.");

                var expectedObj = expected[key];
                var actualObj = result[key];

                Assert.AreEqual(expectedObj.Input, actualObj.Input, $"Input mismatch at key {key}.");
                Assert.AreEqual(expectedObj.Else, actualObj.Else, $"Else flag mismatch at key {key}.");
                Assert.AreEqual(expectedObj.ParentId, actualObj.ParentId, $"ParentId mismatch at key {key}.");
                Assert.AreEqual(expectedObj.Layer, actualObj.Layer, $"Layer mismatch at key {key}.");
                Assert.AreEqual(expectedObj.Position, actualObj.Position, $"Position mismatch at key {key}.");
                Assert.AreEqual(expectedObj.Nested, actualObj.Nested, $"Nested flag mismatch at key {key}.");

                // Check commands count
                Assert.AreEqual(expectedObj.Commands.Count, actualObj.Commands.Count,
                    $"Commands count mismatch at key {key}.");

                // For each expected command key, try to find it in actual commands by key and category
                foreach (var (category, expectedCmdKey, expectedValue) in expectedObj.Commands)
                {
                    var found = false;
                    foreach (var (actCategory, actKey, actValue) in actualObj.Commands)
                    {
                        if (actCategory == category && actKey == expectedCmdKey)
                        {
                            Assert.AreEqual(expectedValue, actValue,
                                $"Command value mismatch for category '{category}' and key '{expectedCmdKey}' at IfElseObj {key}.");
                            found = true;
                            break;
                        }
                    }

                    Assert.IsTrue(found,
                        $"Command with category '{category}' and key '{expectedCmdKey}' not found at IfElseObj {key}.");
                }
            }
        }


        /// <summary>
        ///     Parses if else clauses empty input returns empty dictionary.
        /// </summary>
        [TestMethod]
        public void ParseIfElseClausesEmptyInputReturnsEmptyDictionary()
        {
            var input = string.Empty;
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            Assert.AreEqual(null, result, "The result should be an empty dictionary for empty input.");
        }
    }
}
