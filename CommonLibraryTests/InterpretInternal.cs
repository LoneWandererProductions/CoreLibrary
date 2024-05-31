/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/InterpretInternal.cs
 * PURPOSE:     Tests for the internals of theInterpreter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Interpreter unit test class. Internal functions
    /// </summary>
    [TestClass]
    public sealed class InterpretInternal
    {
        /// <summary>
        /// Singles the check should return true for balanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnTrueForBalancedParentheses()
        {
            var input = "(a + b) * c";
            var result = Irt.SingleCheck(input);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Singles the check should return false for unbalanced parentheses.
        /// </summary>
        [TestMethod]
        public void SingleCheckShouldReturnFalseForUnbalancedParentheses()
        {
            var input = "(a + b * c";
            var result = Irt.SingleCheck(input);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Removes the parenthesis should remove outer parentheses when well formed.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldRemoveOuterParenthesesWhenWellFormed()
        {
            var input = "(abc)";
            var result = Irt.RemoveParenthesis(input, ')', '(');
            Assert.AreEqual("abc", result);
        }

        /// <summary>
        /// Removes the parenthesis should return the input string, this is just an edge Case
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldReturnInput()
        {
            var input = "abc";
            var result = Irt.RemoveParenthesis(input, ')', '(');
            Assert.AreEqual("abc", result);
        }

        /// <summary>
        /// Removes the parenthesis should return error message when input has mismatched parentheses.
        /// </summary>
        [TestMethod]
        public void RemoveParenthesisShouldReturnErrorMessageWhenInputHasMismatchedParentheses()
        {
            var input = "(abc";
            var result = Irt.RemoveParenthesis(input, ')', '(');
            Assert.AreEqual(IrtConst.ParenthesisError, result);
        }

        /// <summary>
        /// Checks the overload should return command identifier when overload matches.
        /// </summary>
        [TestMethod]
        public void CheckOverloadShouldReturnCommandIdWhenOverloadMatches()
        {
            var commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "command", ParameterCount = 2 } }
            };

            var result = Irt.CheckOverload("command", 2, commands);
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Checks the overload should return error when overload does not match.
        /// </summary>
        [TestMethod]
        public void CheckOverloadShouldReturnErrorWhenOverloadDoesNotMatch()
        {
            var commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "command", ParameterCount = 2 } }
            };

            var result = Irt.CheckOverload("command", 3, commands);
            Assert.AreEqual(IrtConst.ErrorParam, result);
        }


    }
}
