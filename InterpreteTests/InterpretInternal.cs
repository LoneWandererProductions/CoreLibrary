/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        InterpreteTests/InterpretInternal.cs
 * PURPOSE:     Tests for the internals of the Interpreter
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using ExtendedSystemObjects;
using Interpreter;
using Interpreter.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    /// <summary>
    ///     Interpreter unit test class. Internal functions
    /// </summary>
    [TestClass]
    public sealed class InterpretInternal
    {
        /// <summary>
        ///     Gets the sample commands.
        /// </summary>
        /// <returns>Basic Command set</returns>
        private static IReadOnlyDictionary<int, InCommand> GetSampleCommands()
        {
            return new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "CMD1" } }, { 2, new InCommand { Command = "CMD2" } }
            };
        }

        /// <summary>
        ///     Tests if SingleCheck correctly identifies balanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnTrueForBalancedParentheses()
        {
            const string input = "(a + b) * c";
            var result = IrtKernel.SingleCheck(input);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Tests if SingleCheck correctly identifies unbalanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnFalseForUnbalancedParentheses()
        {
            const string input = "(a + b * c";
            var result = IrtKernel.SingleCheck(input);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Tests if RemoveParenthesis correctly removes outer parentheses when well-formed.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldRemoveOuterParenthesesWhenWellFormed()
        {
            const string input = "(abc)";
            var result = IrtKernel.RemoveParenthesis(input, '(', ')');
            Assert.AreEqual("abc", result);
        }

        /// <summary>
        ///     Tests if RemoveParenthesis returns the input string when no outer parentheses are present.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldReturnInput()
        {
            const string input = "abc";
            var result = IrtKernel.RemoveParenthesis(input, '(', ')');
            Assert.AreEqual("abc", result);
        }

        /// <summary>
        ///     Tests if RemoveParenthesis returns an error message when input has mismatched parentheses.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldReturnErrorMessageWhenInputHasMismatchedParentheses()
        {
            const string input = "(abc";
            var result = IrtKernel.RemoveParenthesis(input, '(', ')');
            Assert.AreEqual(IrtConst.ParenthesisError, result);
        }

        /// <summary>
        ///     Tests if CheckOverload returns the correct command identifier when overload matches.
        /// </summary>
        [TestMethod]
        public void CheckOverloadShouldReturnCommandIdWhenOverloadMatches()
        {
            var commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "command", ParameterCount = 2 } }
            };

            var result = IrtKernel.CheckOverload("command", 2, commands);
            Assert.AreEqual(1, result);
        }

        /// <summary>
        ///     Tests if CheckOverload returns null when overload does not match.
        /// </summary>
        [TestMethod]
        public void CheckOverloadShouldReturnErrorWhenOverloadDoesNotMatch()
        {
            var commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "command", ParameterCount = 2 } }
            };

            var result = IrtKernel.CheckOverload("command", 3, commands);
            Assert.IsNull(result);
        }

        /// <summary>
        ///     Tests if WellFormedParenthesis correctly removes well-formed parentheses.
        /// </summary>
        [TestMethod]
        public void WellFormedParenthesisRemovesWellFormedParenthesis()
        {
            // Arrange
            var input = "text ) ( , ) txt ) ( , ( more text";
            var expected = "text )( , )txt )( ,( more text";

            // Act
            var result = IrtKernel.WellFormedParenthesis(input);

            // Assert
            Assert.AreEqual(expected, result);

            // Arrange
            input = " text {   internal(12 ,,,44   ) ,  internal4(      ,5)   }  ";
            expected = "text{   internal(12 ,,,44   ),  internal4(      ,5)}";

            // Act
            result = IrtKernel.WellFormedParenthesis(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Removes the parenthesis with malformed parentheses returns error.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisWithMalformedParenthesesReturnsError()
        {
            var result = IrtKernel.RemoveParenthesis("(a + b", '(', ')');

            Assert.AreEqual(IrtConst.ParenthesisError, result);
        }

        /// <summary>
        ///     Checks for key word with non existing command returns error.
        /// </summary>
        [TestMethod]
        public void CheckForKeyWordWithNonExistingCommandReturnsError()
        {
            var commands = new Dictionary<int, InCommand> { { 1, new InCommand { Command = "TEST" } } };

            var result = IrtKernel.CheckForKeyWord("INVALID", commands);

            Assert.AreEqual(IrtConst.Error, result);
        }

        /// <summary>
        ///     Singles the check should return true for nested balanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnTrueForNestedBalancedParentheses()
        {
            const string input = "((a + b) * (c - d))";
            var result = IrtKernel.SingleCheck(input);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Singles the check should return false for different count unbalanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnFalseForDifferentCountUnbalancedParentheses()
        {
            const string input = "(a + b) * (c - d";
            var result = IrtKernel.SingleCheck(input);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the multiple should return true for balanced multiple parentheses.
        /// </summary>
        [TestMethod]
        public void CheckMultipleShouldReturnTrueForBalancedMultipleParentheses()
        {
            const string input = "{(a + b) * [c - d]}";
            var openParenthesis = new[] { '(', '{', '[' };
            var closeParenthesis = new[] { ')', '}', ']' };
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the multiple should return false for unbalanced multiple parentheses.
        /// </summary>
        [TestMethod]
        public void CheckMultipleShouldReturnFalseForUnbalancedMultipleParentheses()
        {
            const string input = "{(a + b) * [c - d}";
            var openParenthesis = new[] { '(', '{', '[' };
            var closeParenthesis = new[] { ')', '}', ']' };
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Removes the parenthesis should remove outer curly braces when well formed.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldRemoveOuterCurlyBracesWhenWellFormed()
        {
            const string input = "{abc}";
            var result = IrtKernel.RemoveParenthesis(input, '{', '}');
            Assert.AreEqual("abc", result);
        }

        /// <summary>
        ///     Checks for key word with existing command returns correct key.
        /// </summary>
        [TestMethod]
        public void CheckForKeyWordWithExistingCommandReturnsCorrectKey()
        {
            var commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "TEST" } }, { 2, new InCommand { Command = "VALID" } }
            };

            var result = IrtKernel.CheckForKeyWord("VALID", commands);
            Assert.AreEqual(2, result);
        }

        /// <summary>
        ///     Splits the parameter single parameter should return single parameter.
        /// </summary>
        [TestMethod]
        public void SplitParameterSingleParameterShouldReturnSingleParameter()
        {
            var result = IrtKernel.SplitParameter("parameter1", ',');
            CollectionAssert.AreEqual(new List<string> { "parameter1" }, result);
        }

        /// <summary>
        ///     Splits the parameter multiple parameters should return all parameters.
        /// </summary>
        [TestMethod]
        public void SplitParameterMultipleParametersShouldReturnAllParameters()
        {
            var result = IrtKernel.SplitParameter("parameter1,parameter2,parameter3", ',');
            CollectionAssert.AreEqual(new List<string> { "parameter1", "parameter2", "parameter3" }, result);
        }

        /// <summary>
        ///     Splits the parameter with spaces should trim spaces.
        /// </summary>
        [TestMethod]
        public void SplitParameterWithSpacesShouldTrimSpaces()
        {
            var result = IrtKernel.SplitParameter(" parameter1 , parameter2 , parameter3 ", ',');
            CollectionAssert.AreEqual(new List<string> { "parameter1", "parameter2", "parameter3" }, result);
        }

        /// <summary>
        ///     Splits the parameter empty parameters should remove empty parameters.
        /// </summary>
        [TestMethod]
        public void SplitParameterEmptyParametersShouldRemoveEmptyParameters()
        {
            var result = IrtKernel.SplitParameter("parameter1,,parameter3", ',');
            CollectionAssert.AreEqual(new List<string> { "parameter1", "parameter3" }, result);
        }

        /// <summary>
        ///     Splits the parameter empty and whitespace parameters should remove empty and whitespace parameters.
        /// </summary>
        [TestMethod]
        public void SplitParameterEmptyAndWhitespaceParametersShouldRemoveEmptyAndWhitespaceParameters()
        {
            var result = IrtKernel.SplitParameter("parameter1, ,parameter3", ',');
            CollectionAssert.AreEqual(new List<string> { "parameter1", "parameter3" }, result);
        }

        /// <summary>
        ///     Splits the parameter custom splitter should split by custom splitter.
        /// </summary>
        [TestMethod]
        public void SplitParameterCustomSplitterShouldSplitByCustomSplitter()
        {
            var result = IrtKernel.SplitParameter("parameter1|parameter2|parameter3", '|');
            CollectionAssert.AreEqual(new List<string> { "parameter1", "parameter2", "parameter3" }, result);
        }

        /// <summary>
        ///     Samples the splitter.
        /// </summary>
        [TestMethod]
        public void SampleSplitter()
        {
            var result = IrtKernel.SplitParameter("help;;;", ';');
            CollectionAssert.AreEqual(new List<string> { "help" }, result);
        }

        /// <summary>
        ///     Tests internal extension functionality.
        /// </summary>
        [TestMethod]
        public void InternalExtensionTest()
        {
            // Arrange
            var input = "   test().    Commmand() ";
            const string expected = "test().    Commmand()";
            var ext = new IrtExtension();

            // Act
            var result = IrtKernel.WellFormedParenthesis(input);

            // Assert
            Assert.AreEqual(expected, result);

            // Act
            var extensions =
                ext.CheckForExtension(result, IrtConst.InternalNameSpace, IrtConst.InternalExtensionCommands);

            // Assert
            Assert.AreEqual(IrtConst.Error, extensions.Status, "Not handled correctly");

            // Arrange
            input = "   test().    use(     ) ";

            // Act
            result = IrtKernel.WellFormedParenthesis(input);
            extensions = ext.CheckForExtension(result, IrtConst.InternalNameSpace, IrtConst.InternalExtensionCommands);

            // Assert
            Assert.AreEqual(IrtConst.ParameterMismatch, extensions.Status, "Not handled correctly");

            // Arrange
            input = "   test().use(test) ";

            // Act
            result = IrtKernel.WellFormedParenthesis(input);
            extensions = ext.CheckForExtension(result, IrtConst.InternalNameSpace, IrtConst.InternalExtensionCommands);

            // Assert
            Assert.AreEqual(IrtConst.ExtensionFound, extensions.Status, "Not handled correctly");

            // Test more exotic displays of extension

            // Arrange
            input = "   test(.).use(test) ";

            // Act
            result = IrtKernel.WellFormedParenthesis(input);
            extensions = ext.CheckForExtension(result, IrtConst.InternalNameSpace, IrtConst.InternalExtensionCommands);

            // Assert
            Assert.AreEqual(IrtConst.ExtensionFound, extensions.Status, "Not handled correctly");
            Assert.AreEqual("test(.)", extensions.Extension.BaseCommand, "Not handled correctly");

            // Arrange
            input = "   test( /...) . use(test)  ";

            // Act
            result = IrtKernel.WellFormedParenthesis(input);
            extensions = ext.CheckForExtension(result, IrtConst.InternalNameSpace, IrtConst.InternalExtensionCommands);

            // Assert
            Assert.AreEqual(IrtConst.ExtensionFound, extensions.Status, "Not handled correctly");
            Assert.AreEqual("test( /...)", extensions.Extension.BaseCommand, "Not handled correctly");
        }

        /// <summary>
        ///     Gets the parameters with advanced open returns batch command.
        /// </summary>
        [TestMethod]
        public void GetParametersWithAdvancedOpenReturnsBatchCommand()
        {
            // Arrange
            const string input = "CMD1{param}";
            const int key = 1;
            var commands = GetSampleCommands();

            // Act
            var (status, parameter) = IrtKernel.GetParameters(input, key, commands);

            // Assert
            Assert.AreEqual(IrtConst.BatchCommand, status);
            Assert.AreEqual("{param}", parameter);
        }

        /// <summary>
        ///     Gets the parameters with normal parameters returns parameter command.
        /// </summary>
        [TestMethod]
        public void GetParametersWithNormalParametersReturnsParameterCommand()
        {
            // Arrange
            const string input = "CMD1(param)";
            const int key = 1;
            var commands = GetSampleCommands();

            // Act
            var (status, parameter) = IrtKernel.GetParameters(input, key, commands);

            // Assert
            Assert.AreEqual(IrtConst.ParameterCommand, status);
            Assert.AreEqual("param", parameter);
        }

        /// <summary>
        ///     Gets the parameters with whitespace returns parameter command.
        /// </summary>
        [TestMethod]
        public void GetParametersWithWhitespaceReturnsParameterCommand()
        {
            // Arrange
            const string input = "   CMD1   (   param   )   ";
            const int key = 1;
            var commands = GetSampleCommands();

            // Act
            var (status, parameter) = IrtKernel.GetParameters(input, key, commands);

            // Assert
            Assert.AreEqual(IrtConst.ParameterCommand, status);
            Assert.AreEqual("param", parameter);
        }

        /// <summary>
        ///     Checks the format valid format returns true.
        /// </summary>
        [TestMethod]
        public void CheckFormatValidFormatReturnsTrue()
        {
            // Arrange
            const string input = "command(label)";
            const string command = "command";
            const string label = "label";

            // Act
            var result = IrtKernel.CheckFormat(input, command, label);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the format invalid format returns false.
        /// </summary>
        [TestMethod]
        public void CheckFormatInvalidFormatReturnsFalse()
        {
            // Arrange
            const string input = "invalid(label)";
            const string command = "command";
            const string label = "label";

            // Act
            var result = IrtKernel.CheckFormat(input, command, label);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the format empty input returns false.
        /// </summary>
        [TestMethod]
        public void CheckFormatEmptyInputReturnsFalse()
        {
            // Arrange
            const string input = "";
            const string command = "command";
            const string label = "label";

            // Act
            var result = IrtKernel.CheckFormat(input, command, label);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the format null input returns false.
        /// </summary>
        [TestMethod]
        public void CheckFormatNullInputReturnsFalse()
        {
            // Arrange
            const string command = "command";
            const string label = "label";

            // Act
            var result = IrtKernel.CheckFormat(null, command, label);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the format different label returns false.
        /// </summary>
        [TestMethod]
        public void CheckFormatDifferentLabelReturnsFalse()
        {
            // Arrange
            const string input = "command(otherlabel)";
            const string command = "command";
            const string label = "label";

            // Act
            var result = IrtKernel.CheckFormat(input, command, label);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Tests the first if with else.
        /// </summary>
        [TestMethod]
        public void TestFirstIfWithElse()
        {
            const string input = "if(condition) {com1; com2;com3;} else {com1; com1; com1;} com1; com1; com1;";
            const int expected = 56; // Position of the last '}'
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected + 1, block.Length);
            Assert.AreEqual(33, elsePosition);
        }

        /// <summary>
        ///     Tests the nested if else.
        /// </summary>
        [TestMethod]
        public void TestNestedIfElse()
        {
            const string input =
                "if(condition) {com1; com2;com3;} else {if(condition) {com1; com2;com3;} else {com1; com1; com1;} com1; com1; } com1;";
            const int expected = 109; // Position of the last '}'
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected + 1, block.Length);
            Assert.AreEqual(33, elsePosition);
        }

        /// <summary>
        ///     Tests if else if else.
        /// </summary>
        [TestMethod]
        public void TestIfElseIfElse()
        {
            const string input =
                "if(condition) {com1; com2;com3;} else {com1; com1; com1;} com1; com1; com1; if(condition) {com1; com2;com3;} else {com1; com1; com1;}";
            const int expected = 57; // Position of the last '}'
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block.Length);
            Assert.AreEqual(33, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else simple nested if else returns correct block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseSimpleNestedIfElseReturnsCorrectBlock()
        {
            const string input = "if(condition) { if(innerCondition) { com1; } else { com2; } } else { com3; }";
            const string expected = "if(condition) { if(innerCondition) { com1; } else { com2; } } else { com3; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(62, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else if without else returns if block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseIfWithoutElseReturnsIfBlock()
        {
            const string input = "if(condition) { com1; } com2; com3;";
            const string expected = "if(condition) { com1; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(-1, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else multiple if else blocks returns first block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseMultipleIfElseBlocksReturnsFirstBlock()
        {
            const string input = "if(condition1) { com1; } else { com2; } if(condition2) { com3; } else { com4; }";
            const string expected = "if(condition1) { com1; } else { com2; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(25, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else no if returns null.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseNoIfReturnsNull()
        {
            const string input = "com1; com2; com3;";
            var result = IrtKernel.ExtractFirstIfElse(input);
            Assert.IsNull(result.block);
        }

        /// <summary>
        ///     Extracts the first if else multiple levels of nesting returns first block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseMultipleLevelsOfNestingReturnsFirstBlock()
        {
            const string input =
                "if(condition) { if(innerCondition1) { com1; } else { if(innerCondition2) { com2; } else { com3; } } } else { com4; }";
            const string expected =
                "if(condition) { if(innerCondition1) { com1; } else { if(innerCondition2) { com2; } else { com3; } } } else { com4; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(102, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else if else with comments returns first block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseIfElseWithCommentsReturnsFirstBlock()
        {
            const string input = "if(condition) { /* comment */ com1; } else { // comment \n com2; } com3;";
            const string expected = "if(condition) { /* comment */ com1; } else { // comment \n com2; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(38, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else at end of string handles correctly.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseElseAtEndOfStringHandlesCorrectly()
        {
            const string input = "if(condition) { com1; } else";
            const string expected = "if(condition) { com1; } else";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(24, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else without braces handles correctly.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseElseWithoutBracesHandlesCorrectly()
        {
            const string input = "if(condition) { com1; } else com2;";
            const string expected = "if(condition) { com1; } else com2;";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(24, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else mixed case returns correct block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseMixedCaseReturnsCorrectBlock()
        {
            const string input = "If(condition) { com1; } eLsE { com2; }";
            const string expected = "If(condition) { com1; } eLsE { com2; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(24, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else upper case returns correct block.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseUpperCaseReturnsCorrectBlock()
        {
            const string input = "IF(condition) { com1; } ELSE { com2; }";
            const string expected = "IF(condition) { com1; } ELSE { com2; }";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(24, elsePosition);
        }

        /// <summary>
        ///     Extracts the first if else no else.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseNoElse()
        {
            const string input = "if(condition) { com1; } com4;"; // Adjusted input string
            const string expected = "if(condition) { com1; }"; // Expected output with no `else`
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(-1, elsePosition); // Since there's no `else`, the position should be -1
        }

        /// <summary>
        ///     Extracts the nested if with no else.
        /// </summary>
        [TestMethod]
        public void ExtractNestedIfNoElse()
        {
            const string input = "if (condition1){command1; if (condition2){command2;}}";
            const string
                expected = "if (condition1){command1; if (condition2){command2;}}"; // Expected output with no `else`
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.AreEqual(expected, block);
            Assert.AreEqual(-1, elsePosition); // Since there's no `else`, the position should be -1
        }

        /// <summary>
        ///     Extracts the first if else empty input returns null.
        /// </summary>
        [TestMethod]
        public void ExtractFirstIfElseEmptyInputReturnsNull()
        {
            const string input = "";
            var (block, elsePosition) = IrtKernel.ExtractFirstIfElse(input);
            Assert.IsNull(block);
        }

        /// <summary>
        ///     Extracts the wrong if expression.
        /// </summary>
        [TestMethod]
        public void ExtractWrongIfExpression()
        {
            var input = "if(condition)";
            var result = IrtKernel.ExtractFirstIfElse(input);
            Assert.IsNull(result.block);

            input = "if(condition { Command1; }";
            result = IrtKernel.ExtractFirstIfElse(input);
            Assert.IsNull(result.block);
        }

        /// <summary>
        ///     Validates the parameters with correct parameters returns true.
        /// </summary>
        [TestMethod]
        public void ValidateParametersWithCorrectParametersReturnsTrue()
        {
            // Arrange
            var commands = new Dictionary<int, InCommand> { { 1, new InCommand { ParameterCount = 2 } } };

            // Act
            var result = IrtKernel.ValidateParameters(1, 2, commands);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Validates the parameters with incorrect parameters returns false.
        /// </summary>
        [TestMethod]
        public void ValidateParametersWithIncorrectParametersReturnsFalse()
        {
            // Arrange
            var commands = new Dictionary<int, InCommand> { { 1, new InCommand { ParameterCount = 2 } } };

            // Act
            var result = IrtKernel.ValidateParameters(1, 3, commands);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Removes the first occurrence removes symbol.
        /// </summary>
        [TestMethod]
        public void RemoveFirstOccurrence_RemovesSymbol()
        {
            // Act
            var result = IrtKernel.RemoveFirstOccurrence("hello world", 'o');

            // Assert
            Assert.AreEqual("hell world", result);
        }

        /// <summary>
        ///     Cuts the last occurrence removes last symbols.
        /// </summary>
        [TestMethod]
        public void CutLastOccurrenceRemovesLastSymbol()
        {
            // Act
            var result = IrtKernel.CutLastOccurrence("hello world", 'o');

            // Assert
            Assert.AreEqual("hello w", result);
        }

        /// <summary>
        ///     Removes the word removes keyword.
        /// </summary>
        [TestMethod]
        public void RemoveWordRemovesKeyword()
        {
            // Act
            var result = IrtKernel.RemoveWord("keyword", "This is a keyword example.");

            // Assert
            Assert.AreEqual("This is a  example.", result);
        }

        /// <summary>
        ///     Extracts the condition removes keyword and parentheses.
        /// </summary>
        [TestMethod]
        public void ExtractConditionRemovesKeywordAndParentheses()
        {
            // Act
            var result = IrtKernel.ExtractCondition("if (condition) something", "if");

            // Assert
            Assert.AreEqual("condition", result);
        }

        /// <summary>
        ///     Extracts the condition non if keyword.
        /// </summary>
        [TestMethod]
        public void ExtractConditionNonIfKeyword()
        {
            // Act
            var result = IrtKernel.ExtractCondition("while (condition) something", "while");

            // Assert
            Assert.AreEqual("condition", result);
        }

        /// <summary>
        ///     Extracts the condition no condition.
        /// </summary>
        [TestMethod]
        public void ExtractConditionNoCondition()
        {
            // Act
            var result = IrtKernel.ExtractCondition("if () something", "if");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        ///     Extracts the condition multiple conditions.
        ///     Not supported right now!
        /// </summary>
        [TestMethod]
        public void ExtractConditionMultipleConditions()
        {
            // Act
            var result = IrtKernel.ExtractCondition("if (condition1 && condition2) something", "if");

            // Assert
            Assert.AreEqual("condition1 && condition2", result);
        }


        /// <summary>
        ///     Extracts the condition keyword case insensitive.
        /// </summary>
        [TestMethod]
        public void ExtractConditionKeywordCaseInsensitive()
        {
            // Act
            var result = IrtKernel.ExtractCondition("IF (condition) something", "if");

            // Assert
            Assert.AreEqual("condition", result);
        }


        /// <summary>
        ///     Finds the first if index finds keyword.
        /// </summary>
        [TestMethod]
        public void FindFirstIfIndexFindsKeyword()
        {
            // Act
            var result = IrtKernel.FindFirstKeywordIndex("some text if (condition)", "if");

            // Assert
            Assert.AreEqual(10, result);

            // Act
            result = IrtKernel.FindFirstKeywordIndex("some text if (condition)", "else");

            // Assert
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        ///     Determines whether [contains keyword with open paren finds keyword with paren].
        /// </summary>
        [TestMethod]
        public void ContainsKeywordWithOpenParenFindsKeywordWithParen()
        {
            // Act
            var result = IrtKernel.ContainsKeywordWithOpenParenthesis("some text if (condition)", "if");

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Gets the command index finds matching command.
        /// </summary>
        [TestMethod]
        public void GetCommandIndexFindsMatchingCommand()
        {
            // Arrange
            var commands = new Dictionary<int, InCommand> { { 1, new InCommand { Command = "COMMAND" } } };

            // Act
            var result = IrtKernel.GetCommandIndex("COMMAND", commands);

            // Assert
            Assert.AreEqual(1, result);
        }

        /// <summary>
        ///     Gets the command index does not find matching command.
        /// </summary>
        [TestMethod]
        public void GetCommandIndexDoesNotFindMatchingCommand()
        {
            // Arrange
            var commands = new Dictionary<int, InCommand> { { 1, new InCommand { Command = "COMMAND" } } };

            // Act
            var result = IrtKernel.GetCommandIndex("UNKNOWN", commands);

            // Assert
            Assert.AreEqual(IrtConst.Error, result);
        }

        /// <summary>
        ///     Gets the blocks no if keyword returns single block.
        /// </summary>
        [TestMethod]
        public void GetBlocksNoIfKeywordReturnsSingleBlock()
        {
            // Arrange
            const string input = "Command1; Command2; Command3";
            var expected = new CategorizedDictionary<int, string>
            {
                { "Command", 0, "Command1" }, { "Command", 1, "Command2" }, { "Command", 2, "Command3" }
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks input with if keyword returns if block.
        /// </summary>
        [TestMethod]
        public void GetBlocksInputWithIfKeywordReturnsIfBlock()
        {
            // Arrange
            const string input = "Command1; if (condition) { Command2; } Command3";
            var expected = new CategorizedDictionary<int, string>
            {
                { "Command", 0, "Command1" },
                { "If_Condition", 1, "condition" },
                { "If", 2, "Command2;" },
                { "Command", 3, "Command3" }
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks input with multiple if blocks returns multiple blocks.
        /// </summary>
        [TestMethod]
        public void GetBlocksInputWithMultipleIfBlocksReturnsMultipleBlocks()
        {
            // Arrange
            const string input =
                "Command1; if( condition1) { Command2; } else { Command3; } if (condition2 ) { Command4; }";
            var expected = new CategorizedDictionary<int, string>
            {
                { "Command", 0, "Command1" },
                { "If_Condition", 1, "condition1" },
                { "If", 2, "Command2;" },
                { "Else", 3, "Command3;" },
                { "If_Condition", 4, "condition2" },
                { "If", 5, "Command4;" }
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks empty input returns empty dictionary.
        /// </summary>
        [TestMethod]
        public void GetBlocksEmptyInputReturnsEmptyDictionary()
        {
            // Arrange
            var input = string.Empty;
            var expected = new CategorizedDictionary<int, string>();

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks input with only if keyword returns empty command blocks.
        /// </summary>
        [TestMethod]
        public void GetBlocksInputWithOnlyIfKeywordReturnsEmptyCommandBlocks()
        {
            // Arrange
            const string input = "if(condition)";
            var expected = new CategorizedDictionary<int, string> { { "Error", 0, "if(condition)" } };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks with duplicate keys ensures the last value is kept.
        /// </summary>
        [TestMethod]
        public void GetBlocksWithDuplicateKeysEnsuresLastValueIsKept()
        {
            // Arrange
            const string input = "Command1; Command2; Command1";
            var expected = new CategorizedDictionary<int, string>
            {
                { "Command", 0, "Command1" },
                { "Command", 1, "Command2" },
                { "Command", 2, "Command1" } // Ensure the last Command1 is present
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks with extra whitespace and special characters.
        /// </summary>
        [TestMethod]
        public void GetBlocksWithWhitespaceAndSpecialCharacters()
        {
            // Arrange
            const string input = "  Command1 ;  if (condition) { Command2; }  ";
            var expected = new CategorizedDictionary<int, string>
            {
                { "Command", 0, "Command1" }, { "If_Condition", 1, "condition" }, { "If", 2, "Command2;" }
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the blocks with invalid syntax or unexpected input.
        /// </summary>
        [TestMethod]
        public void GetBlocksWithInvalidSyntaxReturnsErrorBlock()
        {
            // Arrange
            const string input = "if(condition { Command1; }";
            var expected = new CategorizedDictionary<int, string>
            {
                {
                    "Error", 0, "if(condition { Command1; }"
                } // Expecting the whole input to be treated as an error block
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }

        /// <summary>
        ///     Gets the error sample.
        /// </summary>
        [TestMethod]
        public void GetErrorSample()
        {
            // Arrange
            const string input = "if (condition) { doSomething(); } else { doSomething2(); }";
            var expected = new CategorizedDictionary<int, string>
            {
                { "If_Condition", 0, "condition" }, // This is the content inside the 'if' block
                { "If", 1, "doSomething();" }, // This is the content inside the 'if' block
                { "Else", 2, "doSomething2();" } // This is the content inside the 'else' block
            };

            // Act
            var result = IrtKernel.GetBlocks(input);

            // Assert
            var areEqual = AreEqual(expected, result, out var message);
            Assert.IsTrue(areEqual, message);
        }


        /// <summary>
        ///     Checks the multiple parenthesis balanced parentheses returns true.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisBalancedParenthesesReturnsTrue()
        {
            // Arrange
            const string input = "(([]))";
            var openParenthesis = new[] { '(', '[' };
            var closeParenthesis = new[] { ')', ']' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsTrue(result, "The parentheses should be balanced.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis nested balanced parentheses returns true.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisNestedBalancedParenthesesReturnsTrue()
        {
            // Arrange
            const string input = "{[()]}";
            var openParenthesis = new[] { '{', '[', '(' };
            var closeParenthesis = new[] { '}', ']', ')' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsTrue(result, "The parentheses should be balanced.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis unbalanced parentheses extra opening returns false.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisUnbalancedParenthesesExtraOpeningReturnsFalse()
        {
            // Arrange
            const string input = "([";
            var openParenthesis = new[] { '(', '[' };
            var closeParenthesis = new[] { ')', ']' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsFalse(result, "The parentheses should be unbalanced due to an extra opening parenthesis.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis unbalanced parentheses extra closing returns false.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisUnbalancedParenthesesExtraClosingReturnsFalse()
        {
            // Arrange
            const string input = "())";
            var openParenthesis = new[] { '(' };
            var closeParenthesis = new[] { ')' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsFalse(result, "The parentheses should be unbalanced due to an extra closing parenthesis.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis mismatched parentheses returns false.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisMismatchedParenthesesReturnsFalse()
        {
            // Arrange
            const string input = "({[)]}";
            var openParenthesis = new[] { '{', '[', '(' };
            var closeParenthesis = new[] { '}', ']', ')' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsFalse(result, "The parentheses should be unbalanced due to mismatched parentheses.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis empty input returns true.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisEmptyInputReturnsTrue()
        {
            // Arrange
            const string input = "";
            var openParenthesis = new[] { '(', '[' };
            var closeParenthesis = new[] { ')', ']' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsTrue(result, "An empty string should be considered balanced.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis single parenthesis returns false.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisSingleParenthesisReturnsFalse()
        {
            // Arrange
            const string input = "[";
            var openParenthesis = new[] { '[' };
            var closeParenthesis = new[] { ']' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsFalse(result, "A single parenthesis should be considered unbalanced.");
        }

        /// <summary>
        ///     Checks the multiple parenthesis correctly nested different types returns true.
        /// </summary>
        [TestMethod]
        public void CheckMultipleParenthesisCorrectlyNestedDifferentTypesReturnsTrue()
        {
            // Arrange
            const string input = "({[]})";
            var openParenthesis = new[] { '{', '[', '(' };
            var closeParenthesis = new[] { '}', ']', ')' };

            // Act
            var result = IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);

            // Assert
            Assert.IsTrue(result, "The parentheses should be balanced with correctly nested different types.");
        }

        /// <summary>
        ///     Ares the equal.
        /// </summary>
        /// <typeparam name="TK">The type of the k.</typeparam>
        /// <typeparam name="TV">The type of the v.</typeparam>
        /// <param name="dict1">The dict1.</param>
        /// <param name="dict2">The dict2.</param>
        /// <param name="message">The message.</param>
        /// <returns>Evaluate if Dictionaries are equal.</returns>
        private static bool AreEqual<TK, TV>(CategorizedDictionary<TK, TV> dict1, CategorizedDictionary<TK, TV> dict2,
            out string message)
        {
            // Check if both dictionaries have the same number of entries
            if (dict1.Count != dict2.Count)
            {
                message = $"Dictionaries have different counts: {dict1.Count} vs {dict2.Count}.";
                return false;
            }

            // Get all keys from both dictionaries
            var dict1Keys = new HashSet<TK>(dict1.GetKeys());
            var dict2Keys = new HashSet<TK>(dict2.GetKeys());

            // Ensure both dictionaries have the same keys
            if (!dict1Keys.SetEquals(dict2Keys))
            {
                message = "Dictionaries have different keys.";
                return false;
            }

            // Compare the category and value for each key
            foreach (var key in dict1Keys)
            {
                var dict1Entry = dict1.GetCategoryAndValue(key);
                var dict2Entry = dict2.GetCategoryAndValue(key);

                if (dict1Entry == null || dict2Entry == null)
                {
                    message = $"Key {key} not found in both dictionaries.";
                    return false;
                }

                var dict1Category = dict1Entry.Value.Category;
                var dict1Value = dict1Entry.Value.Value;
                var dict2Category = dict2Entry.Value.Category;
                var dict2Value = dict2Entry.Value.Value;

                if (!EqualityComparer<string>.Default.Equals(dict1Category, dict2Category))
                {
                    message = $"Category mismatch for key {key}: {dict1Category} vs {dict2Category}.";
                    return false;
                }

                if (!EqualityComparer<TV>.Default.Equals(dict1Value, dict2Value))
                {
                    message = $"Value mismatch for key {key}: {dict1Value} vs {dict2Value}.";
                    return false;
                }
            }

            message = "Dictionaries are equal.";
            return true;
        }
    }
}
