/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/FeedbackTests.cs
 * PURPOSE:     Tests for the Feedback loop
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    /// <summary>
    ///     Test user Feedback
    /// </summary>
    [TestClass]
    public sealed class FeedbackTests
    {
        /// <summary>
        ///     The log
        /// </summary>
        private static string _log;

        /// <summary>
        ///     The out command
        /// </summary>
        private static OutCommand _outCommand;

        /// <summary>
        ///     The prompt
        /// </summary>
        private readonly Prompt _prompt = new();

        /// <summary>
        ///     The user feedback
        /// </summary>
        private readonly Dictionary<int, UserFeedback> _userFeedback = new();

        /// <summary>
        ///     Feedback and extension test.
        /// </summary>
        [TestMethod]
        public void FeedbackExtension()
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

            // Act
            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.Initiate(dctCommandOne, "UserSpace 1");
            prompt.ConsoleInput("coM1(1,2).Help()");
            prompt.ConsoleInput("");


            Assert.IsFalse(_log.Contains("Help com1"), "No help provided.");
            // Assert
            Assert.AreEqual("Input was not valid.", _log, "Error was not catched.");

            prompt.ConsoleInput("mehh");

            Assert.AreEqual("Input was not valid.", _log, "Error was not catched.");

            prompt.ConsoleInput(" yeS   ");

            Trace.WriteLine(_outCommand.ToString());

            Assert.IsNotNull(_outCommand, "Out Command was not empty.");

            prompt.ConsoleInput("List().Help()");
            Assert.IsTrue(_log.Contains("You now have the following Options:"), "Wrong Options provided.");

            prompt.ConsoleInput("YeS   ");

            Assert.IsTrue(_log.Contains("Special case"), "Wrong Commands listed");

            Trace.WriteLine(_log);
            Trace.WriteLine(_outCommand.ToString());
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
            if (e == null)
            {
                _log = string.Empty;
                _outCommand = null;
                return;
            }

            if (e.Command == -1)
            {
                _log = e.ErrorMessage;
            }

            _outCommand = e;
        }
    }
}
