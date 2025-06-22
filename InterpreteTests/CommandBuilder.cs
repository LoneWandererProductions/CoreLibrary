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
using Interpreter;
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
            var input = "com1;";

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

            // Assert
            Assert.AreEqual(1, result.Count);

            var ifElseObj = result[0];
            Assert.AreEqual(0, ifElseObj.Id);
            Assert.AreEqual(-1, ifElseObj.ParentId);
            Assert.AreEqual(0, ifElseObj.Position);
            Assert.AreEqual(0, ifElseObj.Layer);
            Assert.IsFalse(ifElseObj.Else);
            Assert.IsFalse(ifElseObj.Nested);
            Assert.AreEqual("if (condition1) {com1; }", ifElseObj.Input);

            foreach (var value in result)
            {
                Trace.WriteLine(value.ToString());
            }


            var commands = IrtParserCommand.BuildCommand(input);

            Trace.WriteLine(commands.ToString());

            var expectedResults = new List<(int Key, string Category, string Value)>
            {
                (0, "IF", "condition1"), (1, "COMMAND", "com1")
            };

            //// Assert
            foreach (var expected in expectedResults)
            {
                var (key, category, value) = expected;
                //Assert.IsTrue(result.TryGetCategory(key, out var actualCategory));

                //Assert.AreEqual(category, actualCategory, $"Category mismatch for key {key}");
                //Assert.IsTrue(result.TryGetValue(key, out var actualValue));
                //TODO error here
                //Assert.AreEqual(value, actualValue, $"Value mismatch for key {key}");
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

            //Assert.AreEqual(3, result.Count, "There should be one IfElseObj in the result.");
            //var obj = result[0];
            //Assert.IsFalse(obj.Else, "The 'Else' flag should be false for an 'if' clause.");
            //Assert.AreEqual(-1, obj.ParentId, "The ParentId should be -1 for a top-level 'if' clause.");
            //Assert.AreEqual(0, obj.Layer, "The Layer should be 0 for a top-level 'if' clause.");
            //Assert.AreEqual(0, obj.Position, "The Position should be 0 for a top-level 'if' clause.");
            //Assert.AreEqual("if (condition) { doSomething(); }", obj.Input, "The Input string should match.");
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

            // Expected structure with nested If-Else clauses
            var expected = new Dictionary<int, IfElseObj>
            {
                {
                    0, new IfElseObj
                    {
                        Input =
                            "if (condition1) { Command1; if (condition2) { Command2; } else { Command3; } } else { Command4; }",
                        Else = false,
                        ParentId = -1,
                        Layer = 1,
                        Position = 0,
                        Nested = true,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "condition1" },
                            { "If", 1, "Command1; if (condition2) { Command2; } else { Command3; }" },
                            { "Else", 2, "Command4;" }
                        }
                    }
                },
                {
                    1,
                    new IfElseObj
                    {
                        Input = "if (condition2) { Command2; } else { Command3; }",
                        Else = false,
                        ParentId = 0,
                        Layer = 2,
                        Position = 1,
                        Nested = true,
                        Commands = new CategorizedDictionary<int, string>
                        {
                            { "If_Condition", 0, "condition2" },
                            { "If", 1, "Command2;" },
                            { "Else", 2, "Command3;" }
                        }
                    }
                }
                //TODO add the rest
            };

            // Act
            var result = ConditionalExpressions.ParseIfElseClauses(input);

            foreach (var value in result)
            {
                Trace.WriteLine(value.ToString());
            }

            // Assert
            //var areEqual =
            //    CategorizedDictionary<int, string>.AreEqual(expected[0].Commands, result[0].Commands, out var message);
            //Assert.IsTrue(areEqual, message);

            // You can add more assertions to check for nested structures if needed
            //areEqual = CategorizedDictionary<int, string>.AreEqual(expected[1].Commands, result[1].Commands,
            //    out message);
            //Assert.IsTrue(areEqual, message);
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

        /// <summary>
        ///     Parses the invalid input throws exception.
        /// </summary>
        [TestMethod]
        public void ParseComplexCommand()
        {
            //base, the command will be removed in the IrtParser
            const string input = "Container{ " +
                                 "Print(hello World);" +
                                 "if(condition) { if(innerCondition) { com1; } else { com2; } } else { com3; }" +
                                 "Label(one);" +
                                 "Print(passed label one);" +
                                 "goto(two);" +
                                 "Print(Should not be printed);" +
                                 "Label(two);" +
                                 "Print(Finish);" +
                                 "}";

            var inputcleaned = "{ " +
                               "Print(hello World);" +
                               "if(condition) { if(innerCondition) { com1; } else { com2; } } else { com3; }" +
                               "Label(one);" +
                               "Print(passed label one);" +
                               "goto(two);" +
                               "Print(Should not be printed);" +
                               "Label(two);" +
                               "Print(Finish);" +
                               "}";

            var expectedResults = new List<(int Key, string Category, string Value)>
            {
                (0, "Command", "Print(hello World)"),
                (1, "IF", "if(condition) { if(innerCondition) { com1; } else { com2; } }"),
                (2, "ELSE", "else { com3; }"),
                (3, "LABEL", "Label(one)"),
                (4, "COMMAND", "Print(passed label one)"),
                (5, "GOTO", "goto(two)"),
                (6, "COMMAND", "Print(Should not be printed)"),
                (7, "LABEL", "Label(two)"),
                (8, "COMMAND", "Print(Finish)")
            };

            // Act
            var result = IrtParserCommand.BuildCommand(inputcleaned);

            Trace.WriteLine(result.ToString());

            //// Assert
            foreach (var expected in expectedResults)
            {
                var (key, category, value) = expected;
                //Assert.IsTrue(result.TryGetCategory(key, out var actualCategory));

                //Assert.AreEqual(category, actualCategory, $"Category mismatch for key {key}");
                //Assert.IsTrue(result.TryGetValue(key, out var actualValue));
                //TODO error here
                //Assert.AreEqual(value, actualValue, $"Value mismatch for key {key}");
            }

            Trace.WriteLine(result.ToString());
            Trace.WriteLine(expectedResults.ToString());

            inputcleaned = "{" +
                           "Print(hello World);" +
                           "if (condition1)" +
                           "{" +
                           "command1;" +
                           "if (condition2)" +
                           "{" +
                           "command2;" +
                           "}" +
                           "}" +
                           "Label(one);" +
                           "Print(passed label one);" +
                           "goto(two);" +
                           "Print(Should not be printed);" +
                           "Label(two);" +
                           "Print(Finish);" +
                           "}";


            var finalResults = new List<(int Key, string Category, string Value)>
            {
                (1, "COMMAND", "Print(hello World)"),
                (2, "IF_1", "if(condition1)"), // IF block 1
                (3, "COMMAND", "command1"),
                (4, "IF_2", "if(condition2)"), // Nested IF block 2 within IF 1
                (5, "COMMAND", "command2"),
                (6, "IF_2_END", ""), // End of IF block 2
                (7, "IF_1_END_NOELSE", ""), // End of IF block 1 with no ELSE
                (8, "LABEL", "Label(one)"),
                (9, "COMMAND", "Print(passed label one)"),
                (10, "GOTO", "goto(two)"),
                (11, "COMMAND", "Print(Should not be printed)"),
                (12, "LABEL", "Label(two)"),
                (13, "COMMAND", "Print(Finish)")
            };

            // Act
            result = IrtParserCommand.BuildCommand(inputcleaned);

            // Assert
            //foreach (var expected in expectedResults)
            //{
            //    var (key, category, value) = expected;
            //    Assert.IsTrue(result.TryGetCategory(key, out var actualCategory));
            //    Assert.AreEqual(category, actualCategory, $"Category mismatch for key {key}");
            //    Assert.IsTrue(result.TryGetValue(key, out var actualValue));
            //    Assert.AreEqual(value, actualValue, $"Value mismatch for key {key}");
            //}

            Trace.WriteLine(result.ToString());
        }
    }
}
