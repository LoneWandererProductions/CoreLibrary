/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Interpret.cs
 * PURPOSE:     Tests for Interpreter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Interpreter unit test class.
    /// </summary>
    [TestClass]
    public sealed class Interpret
    {
        /// <summary>
        ///     The UserSpaceOne (const). Value: "NameSpaceOne".
        /// </summary>
        private const string UserSpaceOne = "NameSpaceOne";

        /// <summary>
        ///     The UserSpaceTwo (const). Value: "NameSpaceTwo".
        /// </summary>
        private const string UserSpaceTwo = "NameSpaceTwo";

        /// <summary>
        ///     The prompt.
        /// </summary>
        private static Prompt _prompt;

        /// <summary>
        ///     The out command.
        /// </summary>
        private static OutCommand _outCommand;

        /// <summary>
        ///     The log.
        /// </summary>
        private static string _log;

        /// <summary>
        ///     The batch
        /// </summary>
        private static readonly string Batch = Path.Combine(Directory.GetCurrentDirectory(), "MyTest.txt");

        /// <summary>
        ///     The Command Dictionary
        /// </summary>
        private static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            { 0, new InCommand { Command = "First", ParameterCount = 2, Description = "Help First" } },
            { 1, new InCommand { Command = "Second", ParameterCount = 3, Description = "Help Second" } },
            {
                2,
                new InCommand { Command = "Third", ParameterCount = 0, Description = "Special case no Parameter" }
            }
        };

        /// <summary>
        ///     The alternate Command Dictionary
        /// </summary>
        private static readonly Dictionary<int, InCommand> DctCommandTwo = new()
        {
            { 4, new InCommand { Command = "Test", ParameterCount = 0, Description = "Here we go" } }
        };

        /// <summary>
        ///     The commands
        /// </summary>
        private Dictionary<int, InCommand> _commands;

        /// <summary>
        ///     The irt prompt
        /// </summary>
        private IrtPrompt _irtPrompt;

        /// <summary>
        ///     The namespace
        /// </summary>
        private string _namespace;

        /// <summary>
        ///     Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            _irtPrompt = new IrtPrompt(new Prompt());
            _commands = new Dictionary<int, InCommand>
            {
                { 1, new InCommand { Command = "COMMAND1", ParameterCount = 0, Description = "Description1" } },
                { 2, new InCommand { Command = "COMMAND2", ParameterCount = 1, Description = "Description2" } }
            };
            _namespace = "TestNamespace";
            var userSpace = new UserSpace { Commands = _commands, UserSpaceName = _namespace };
            _irtPrompt.Initiate(userSpace);
        }

        /// <summary>
        ///     Check our Interpreter
        /// </summary>
        [TestMethod]
        public void ConsoleInterpreter()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);
            _prompt.StartConsole("FirSt(1,2)");

            Assert.AreEqual(0, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("Third()");

            Assert.AreEqual(2, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("third");

            Assert.AreEqual(2, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("helP");

            Assert.IsTrue(_log.Contains("Basic Syntax"), "Wrong Basic Help: " + _log);

            _prompt.StartConsole("helP()");

            Assert.AreEqual(true,
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help not displayed" + _log);

            _prompt.StartConsole("helP(fIrst)");

            Assert.AreEqual(true, _log.Contains("First Description"), "Wrong Help: " + _log);

            _prompt.Dispose();
        }

        /// <summary>
        ///     Check our Interpreter with wrong input
        /// </summary>
        [TestMethod]
        public void BreakConsoleInterpreter()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);
            _prompt.StartConsole("FirSt(1)");

            Assert.IsTrue(_log.Contains("Error in the Syntax"), "Syntax Error: " + _log);
            Assert.AreEqual(-1, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("First(1,2)");

            //Overload

            Assert.AreEqual(0, _outCommand.Command, "Wrong Id: " + _outCommand.Command);
            Assert.AreEqual(2, _outCommand.Parameter.Count, "Wrong Number: " + _outCommand.Parameter.Count);

            _prompt.StartConsole(string.Empty);

            Assert.IsTrue(_log.Contains("error KeyWord not Found: "), "Syntax Error: " + _log);
            Assert.AreEqual(-1, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("FirSt(1,2");

            Assert.IsTrue(_log.Contains("Wrong parenthesis"), "Parenthesis Error: " + _log);
            Assert.AreEqual(-1, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            //possible Break

            _prompt.StartConsole("Third");

            Assert.IsTrue(_log.Contains("Wrong parenthesis"), "Parenthesis Error: " + _log);

            //Internal Test

            _prompt.StartConsole("help");

            Assert.AreNotEqual("Error in the Syntax", _log, "Parenthesis Error: " + _log);

            _prompt.StartConsole("help()");

            Assert.AreEqual(true,
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help not displayed" + _log);
            _prompt.Dispose();
        }

        /// <summary>
        ///     Check overload.
        /// </summary>
        [TestMethod]
        public void Overload()
        {
            var dct = new Dictionary<int, InCommand>();
            var command = new InCommand { Command = "First", ParameterCount = 0, Description = "Help First" };
            dct.Add(0, command);
            command = new InCommand { Command = "First", ParameterCount = 1, Description = "Help Second" };
            dct.Add(1, command);

            //base

            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(dct, UserSpaceOne);
            _prompt.StartConsole("FirSt()");

            Assert.AreEqual(0, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            //overload
            _prompt.StartConsole("FirSt(1)");

            Assert.AreEqual(1, _outCommand.Command, "Wrong Id: " + _outCommand.Command);
            Assert.AreEqual(1, _outCommand.Parameter.Count, "Wrong Number: " + _outCommand.Parameter.Count);

            //break
            _prompt.StartConsole("FirSt(1,3)");

            Assert.AreEqual(-1, _outCommand.Command, "Wrong Id: " + _outCommand.Command);
            Assert.IsTrue(_log.Contains("Error in the Syntax"), "Syntax Error: " + _log);

            _prompt.Dispose();
        }

        /// <summary>
        ///     Check our Interpreter
        /// </summary>
        [TestMethod]
        public void ConsoleNameSpaceSwitch()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);
            _prompt.StartConsole("FirSt(1,2)");

            Assert.AreEqual(0, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.AddCommands(DctCommandTwo, UserSpaceTwo);

            Assert.AreEqual(2, _prompt.CollectedSpaces.Count, "Wrong Number of Namespaces");

            _prompt.StartConsole("use (" + UserSpaceTwo + ")");
            _prompt.StartConsole("test");

            Assert.AreEqual(4, _outCommand.Command, "Wrong Id: " + _outCommand.Command);

            _prompt.StartConsole("using");

            Assert.IsTrue(_log.StartsWith(UserSpaceTwo.ToUpper(), StringComparison.Ordinal), "Namespace not switched");
            _prompt.Dispose();
        }

        /// <summary>
        ///     Check our Interpreter
        /// </summary>
        [TestMethod]
        public void Container()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);
            _prompt.StartConsole("Container{FirSt(1,2); Third() ; ; --test comment;Help()};;;;");

            Assert.AreEqual(true,
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help not displayed" + _log);

            Assert.AreEqual(_prompt.Log[1], "0", "Correct Parameter");
            Assert.AreEqual(_prompt.Log[3], "2", "Correct Parameter");
            _prompt.Dispose();
        }

        /// <summary>
        ///     Execute some Batch File
        /// </summary>
        [TestMethod]
        public void BatchCommand()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Initiate(DctCommandOne, UserSpaceOne);

            CreateFile();

            _prompt.StartConsole("BATCHEXECUTE(" + Batch + ")");

            Assert.AreEqual(true,
                _log.Contains("Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive"),
                "Help not displayed" + _log);

            Assert.AreEqual(_prompt.Log[1], "0", "Correct Parameter");
            Assert.AreEqual(_prompt.Log[2], "Third()", "Correct Parameter");
            Assert.AreEqual(_prompt.Log[3], "2", "Correct Parameter");

            if (File.Exists(Batch))
            {
                File.Delete(Batch);
            }

            _prompt.Dispose();
        }

        /// <summary>
        ///     Creates the file.
        /// </summary>
        private static void CreateFile()
        {
            if (File.Exists(Batch))
            {
                File.Delete(Batch);
            }

            // Create a file to write to.
            using var sw = File.CreateText(Batch);
            sw.WriteLine("FirSt(1,2);");
            sw.WriteLine("Third();");
            sw.WriteLine("Help();");
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
            _outCommand = e;
        }

        /// <summary>
        ///     Handles the input valid command no parameters success.
        /// </summary>
        [TestMethod]
        public void HandleInputValidCommandNoParameters_Success()
        {
            var commandHandled = false;
            _irtPrompt.sendCommand += (sender, e) =>
            {
                Assert.AreEqual(1, e.Command);
                commandHandled = true;
            };

            _irtPrompt.HandleInput("COMMAND1");

            Assert.IsTrue(commandHandled);
        }

        /// <summary>
        ///     Handles the input invalid command logs error.
        /// </summary>
        [TestMethod]
        public void HandleInputInvalidCommandLogsError()
        {
            var logHandled = false;
            _irtPrompt.SendLog += (_, e) =>
            {
                Assert.IsTrue(e.Contains(IrtConst.KeyWordNotFoundError));
                logHandled = true;
            };

            _irtPrompt.HandleInput("INVALIDCOMMAND");

            Assert.IsTrue(logHandled);
        }

        /// <summary>
        ///     Handles the input help command logs help.
        /// </summary>
        [TestMethod]
        public void HandleInputHelpCommandLogsHelp()
        {
            var logHandled = false;
            _irtPrompt.SendLog += (sender, e) =>
            {
                Assert.AreEqual(IrtConst.HelpGeneric, e);
                logHandled = true;
            };

            _irtPrompt.HandleInput(IrtConst.InternalCommandHelp);

            Assert.IsTrue(logHandled);
        }

        /// <summary>
        ///     Handles the input command with parameters success.
        /// </summary>
        [TestMethod]
        public void HandleInputCommandWithParametersSuccess()
        {
            var commandHandled = false;
            _irtPrompt.sendCommand += (sender, e) =>
            {
                Assert.AreEqual(2, e.Command);
                Assert.AreEqual(1, e.Parameter.Count);
                Assert.AreEqual("PARAM1", e.Parameter[0]);
                commandHandled = true;
            };

            _irtPrompt.HandleInput("COMMAND2(PARAM1)");

            Assert.IsTrue(commandHandled);
        }

        /// <summary>
        ///     Handles the input command with invalid parameters logs error.
        /// </summary>
        [TestMethod]
        public void HandleInputCommandWithInvalidParametersLogsError()
        {
            var logHandled = false;
            _irtPrompt.SendLog += (sender, e) =>
            {
                Assert.IsTrue(e.Contains(IrtConst.ParenthesisError));
                logHandled = true;
            };

            _irtPrompt.HandleInput("COMMAND2(PARAM1");

            Assert.IsTrue(logHandled);
        }
    }
}
