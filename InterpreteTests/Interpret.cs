/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        InterpreteTests/InterpreterCompleteTests.cs
 * PURPOSE:     Mostly complete Tests
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    /// <summary>
    ///     Mostly complete batch Tests
    /// </summary>
    [TestClass]
    public sealed class InterpreterCompleteTests
    {
        /// <summary>
        ///     The user space one
        /// </summary>
        private const string UserSpaceOne = "NameSpaceOne";

        /// <summary>
        ///     The user space two
        /// </summary>
        private const string UserSpaceTwo = "NameSpaceTwo";

        /// <summary>
        ///     The prompt
        /// </summary>
        private static Prompt _prompt;

        /// <summary>
        ///     The out command
        /// </summary>
        private static OutCommand _outCommand;

        /// <summary>
        ///     The log
        /// </summary>
        private static string _log;

        /// <summary>
        ///     The batch
        /// </summary>
        private static readonly string Batch = Path.Combine(Directory.GetCurrentDirectory(), "MyTest.txt");

        /// <summary>
        ///     The DCT command one
        /// </summary>
        private static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            { 0, new InCommand { Command = "com1", ParameterCount = 2, Description = "Help com1" } },
            { 1, new InCommand { Command = "com2", ParameterCount = 3, Description = "Help com2" } },
            { 2, new InCommand { Command = "com3", ParameterCount = 0, Description = "Special case no Parameter" } }
        };

        /// <summary>
        ///     The DCT command two
        /// </summary>
        private static readonly Dictionary<int, InCommand> DctCommandTwo = new()
        {
            { 4, new InCommand { Command = "Test", ParameterCount = 0, Description = "Here we go" } }
        };

        /// <summary>
        ///     Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);
        }

        /// <summary>
        ///     Tears down.
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            _prompt?.Dispose();
            if (File.Exists(Batch))
            {
                File.Delete(Batch);
            }
        }

        /// <summary>
        ///     Consoles the interpreter should execute commands.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldExecuteCommands()
        {
            // Arrange
            _prompt.ConsoleInput("coM1(1,2)");

            // Act & Assert
            Assert.AreEqual(0, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("com3()");
            Assert.AreEqual(2, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("cOm3");
            Assert.AreEqual(2, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("helP");
            Assert.IsTrue(_log.Contains("Basic Syntax"), "Help output mismatch.");

            _prompt.ConsoleInput("helP()");
            Assert.IsTrue(
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help output mismatch.");

            _prompt.ConsoleInput("helP(CoM1)");
            Assert.IsTrue(_log.Contains("com1 Description"), "Help output mismatch.");
        }

        /// <summary>
        ///     Consoles the interpreter should handle syntax errors.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldHandleSyntaxErrors()
        {
            // Arrange
            _prompt.ConsoleInput("coM1(1)");

            // Act & Assert
            Assert.IsTrue(_log.Contains("Problems with the provided parameter cound the and the expected amount."), "Syntax error message mismatch.");
            Assert.AreEqual(-1, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("com1(1,2)");
            Assert.AreEqual(0, _outCommand.Command, "Command ID mismatch.");
            Assert.AreEqual(2, _outCommand.Parameter.Count, "Parameter count mismatch.");

            _prompt.ConsoleInput(string.Empty);
            Assert.IsTrue(_log.Contains("Input was null or empty"), "Empty input error message mismatch.");
            Assert.AreEqual(-1, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("coM1(1,2");
            Assert.IsTrue(_log.Contains("Wrong parenthesis"), "Parenthesis error message mismatch.");
            Assert.AreEqual(-1, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("com3");
            Assert.IsTrue(_log.Contains("Wrong parenthesis"), "Parenthesis error message mismatch.");

            _prompt.ConsoleInput("help");
            Assert.AreNotEqual("Error in the Syntax", _log, "Unexpected syntax error message.");

            _prompt.ConsoleInput("help()");
            Assert.IsTrue(
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help output mismatch.");
        }

        /// <summary>
        ///     Consoles the interpreter should handle command overloading.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldHandleCommandOverloading()
        {
            // Arrange
            var commands = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 0, Description = "Help com1" } },
                { 1, new InCommand { Command = "com1", ParameterCount = 1, Description = "Help com2" } }
            };

            _prompt.Initiate(commands, UserSpaceOne);

            // Act & Assert
            _prompt.ConsoleInput("coM1()");
            Assert.AreEqual(0, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("coM1(1)");
            Assert.AreEqual(1, _outCommand.Command, "Command ID mismatch.");
            Assert.AreEqual(1, _outCommand.Parameter.Count, "Parameter count mismatch.");

            _prompt.ConsoleInput("coM1(1,3)");
            Assert.AreEqual(-1, _outCommand.Command, "Command ID mismatch.");
            Assert.IsTrue(_log.Contains("Problems with the provided parameter cound the and the expected amount."), "Syntax error message mismatch.");
        }

        /// <summary>
        ///     Consoles the interpreter should switch namespaces.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldSwitchNamespaces()
        {
            // Arrange
            _prompt.AddCommands(DctCommandTwo, UserSpaceTwo);

            // Act
            _prompt.ConsoleInput("coM1(1,2)");
            Assert.AreEqual(0, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("use (" + UserSpaceTwo + ")");
            _prompt.ConsoleInput("test");
            Assert.AreEqual(4, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("using");
            Assert.IsTrue(_log.StartsWith(UserSpaceTwo.ToUpper(), StringComparison.Ordinal),
                "Namespace switch mismatch.");
        }

        /// <summary>
        ///     Consoles the interpreter should handle namespace switch full test.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldHandleNamespaceSwitchFullTest()
        {
            // Arrange
            var dctCommandOne = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 2, Description = "Help com1" } },
                {
                    1,
                    new InCommand { Command = "com2", ParameterCount = 0, Description = "com2 Command Namespace 1" }
                },
                {
                    2,
                    new InCommand
                    {
                        Command = "com3", ParameterCount = 0, Description = "Special case no Parameter"
                    }
                }
            };

            var dctCommandTwo = new Dictionary<int, InCommand>
            {
                {
                    1, new InCommand { Command = "com2", ParameterCount = 0, Description = "com2 Command Namespace 2" }
                },
                { 4, new InCommand { Command = "Test", ParameterCount = 0, Description = "Here we go" } }
            };

            var extension = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "Ext", ParameterCount = 0, Description = "Null" } },
                { 4, new InCommand { Command = "Ext", ParameterCount = 1, Description = "Overload" } }
            };

            _prompt.Initiate(dctCommandOne, "UserSpace 1");
            _prompt.ConsoleInput("com1(1,2)");

            // Act
            _prompt.AddCommands(dctCommandTwo, "UserSpace 2");
            _prompt.ConsoleInput("use (UserSpace 2)");
            _prompt.ConsoleInput("test");
            Assert.AreEqual(4, _outCommand.Command, "Command ID mismatch.");

            _prompt.ConsoleInput("com2().use(UserSpace 1)");
            Assert.AreEqual(1, _outCommand.Command, "Command ID mismatch.");
            Assert.AreEqual("UserSpace 1", _outCommand.UsedNameSpace, "Namespace mismatch.");

            // Reboot with user extension
            _prompt.Initiate(dctCommandOne, "UserSpace 1", extension);
            _prompt.ConsoleInput("com1(1,2).ext()");
            Assert.AreEqual(1, _outCommand.ExtensionCommand.ExtensionCommand, "Extension Command ID mismatch.");

            _prompt.ConsoleInput("com1(1,2).ext(3)");
            Assert.AreEqual(4, _outCommand.ExtensionCommand.ExtensionCommand, "Extension Command ID mismatch.");
        }

        /// <summary>
        ///     Consoles the interpreter should handle container commands.
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreterShouldHandleContainerCommands()
        {
            // Arrange
            _prompt.ConsoleInput("Container{ Help() };");

            // Act & Assert
            Assert.IsTrue(_log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer)"),
                "Help output mismatch.");

            _prompt.ConsoleInput("Container{ coM1(1,2); com3(); ; ; --test comment; Help() };;;;");
            Assert.IsTrue(
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help output mismatch.");
            Assert.AreEqual("0", _prompt.Log[3], "Command result mismatch.");
        }

        /// <summary>
        ///     Listen to Messages
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private static void SendLogs(object sender, string e)
        {
            Trace.WriteLine(e);
            _log = e;
        }

        /// <summary>
        ///     Listen to Commands
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private static void SendCommands(object sender, OutCommand e)
        {
            if (e.Command == -1)
            {
                _log = e.ErrorMessage;
            }

            _outCommand = e;
        }
    }
}
