/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterpreteTests
 * FILE:        IrtParserInputTests.cs
 * PURPOSE:     Here we test the Parser part of my Script Engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using System.Collections.Generic;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    /// <summary>
    ///     IrtParserInput Test Class
    /// </summary>
    [TestClass]
    public class IrtParserInputTests
    {
        /// <summary>
        ///     The parser input
        /// </summary>
        private IrtParserInput _parserInput;

        /// <summary>
        ///     The prompt
        /// </summary>
        private Prompt _prompt;

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _prompt = new Prompt();
            _parserInput = new IrtParserInput(_prompt);

            var commandDict = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 0 } },
                { 1, new InCommand { Command = "help", ParameterCount = 0 } }
            };

            IrtParserInput.SwitchUserSpace(new UserSpace { Commands = commandDict, UserSpaceName = "TestSpace" });
        }

        /// <summary>
        ///     Switches the user space changes user space.
        /// </summary>
        [TestMethod]
        public void SwitchUserSpaceChangesUserSpace()
        {
            var newCommands = new Dictionary<int, InCommand>
            {
                { 5, new InCommand { Command = "newcmd", ParameterCount = 0 } }
            };

            var newSpace = new UserSpace { UserSpaceName = "NewSpace", Commands = newCommands };

            IrtParserInput.SwitchUserSpace(newSpace);

            var result = _parserInput.ProcessInput("newcmd()");
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Command);
            Assert.AreEqual("NewSpace", result.UsedNameSpace);
        }

        [TestMethod]
        public void ProcessInputReturnsOutCommandForValidCommandWithParameters()
        {
            // Arrange
            var commandDict = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 2, Description = "Help com1" } }
            };

            IrtParserInput.SwitchUserSpace(new UserSpace { Commands = commandDict, UserSpaceName = "DefaultSpace" });

            const string input = "com1(1,2)";

            // Act
            var result = _parserInput.ProcessInput(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Command);
            CollectionAssert.AreEqual(new List<string> { "1", "2" }, result.Parameter);
            Assert.AreEqual("DefaultSpace", result.UsedNameSpace);
        }

        /// <summary>
        ///     Processes the input returns null for unknown command.
        /// </summary>
        [TestMethod]
        public void ProcessInputReturnsNullForUnknownCommand()
        {
            // Arrange
            var commandDict = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 2 } }
            };

            IrtParserInput.SwitchUserSpace(new UserSpace { Commands = commandDict, UserSpaceName = "DefaultSpace" });

            const string input = "unknown(1,2)";

            // Act
            var result = _parserInput.ProcessInput(input);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        ///     Processes the input returns null for wrong parameter count.
        /// </summary>
        [TestMethod]
        public void ProcessInputReturnsNullForWrongParameterCount()
        {
            // Arrange
            var commandDict = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 2 } }
            };

            IrtParserInput.SwitchUserSpace(new UserSpace { Commands = commandDict, UserSpaceName = "DefaultSpace" });

            const string input = "com1(1)"; // only 1 parameter, should be 2

            // Act
            var result = _parserInput.ProcessInput(input);

            // Assert
            Assert.IsNull(result); // error is set inside parser
        }

        /// <summary>
        ///     Processes the input handles single parameter command.
        /// </summary>
        [TestMethod]
        public void ProcessInputHandlesSingleParameterCommand()
        {
            // Arrange
            var commandDict = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "com2", ParameterCount = 1 } }
            };

            IrtParserInput.SwitchUserSpace(new UserSpace { Commands = commandDict, UserSpaceName = "Space2" });

            const string input = "com2(123)";

            // Act
            var result = _parserInput.ProcessInput(input);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Command);
            CollectionAssert.AreEqual(new List<string> { "123" }, result.Parameter);
        }

        /// <summary>
        ///     Handles the input help command works.
        /// </summary>
        [TestMethod]
        public void HandleInputHelpCommandWorks()
        {
            _parserInput.HandleInput("help()");
            Assert.IsTrue(true); // Should process help logic safely
        }

        /// <summary>
        ///     Handles the input empty line handled gracefully.
        /// </summary>
        [TestMethod]
        public void HandleInputEmptyLineHandledGracefully()
        {
            _parserInput.HandleInput("");
            Assert.IsTrue(true); // Should not throw
        }

        /// <summary>
        ///     Disposes the can be called multiple times safely.
        /// </summary>
        [TestMethod]
        public void DisposeCanBeCalledMultipleTimesSafely()
        {
            _parserInput.Dispose();
            _parserInput.Dispose(); // Should be idempotent
            Assert.IsTrue(true);
        }
    }
}
