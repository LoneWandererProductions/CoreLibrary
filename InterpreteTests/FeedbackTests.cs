/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/FeedbackTests.cs
 * PURPOSE:     Tests for the Feedback loop
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Interpreter;
using Interpreter.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterpreteTests
{
    /// <summary>
    ///     Test user Feedback and extension command handling
    /// </summary>
    [TestClass]
    public sealed class FeedbackTests
    {
        private StringBuilder _logBuilder = null!;
        private OutCommand? _outCommand;
        private IrtFeedbackInputEventArgs? _feedback;

        /// <summary>
        /// Predefined feedback message with options
        /// </summary>
        private static readonly UserFeedback ReplaceFeedback = new()
        {
            Before = true,
            Message = "Do you want to commit the following changes?",
            Options = new Dictionary<AvailableFeedback, string>
            {
                { AvailableFeedback.Yes, "If you want to execute the Command type yes" },
                { AvailableFeedback.No, "If you want to stop executing the Command type no." }
            }
        };

        /// <summary>
        /// Feedback dictionary for commands needing user input
        /// </summary>
        internal static readonly Dictionary<int, UserFeedback> Feedback = new() { { 1, ReplaceFeedback } };


        /// <summary>
        /// Runs before each test to reset state
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _logBuilder = new StringBuilder();
            _outCommand = null;
            _feedback = null;
        }


        /// <summary>
        /// Log listener accumulates all log messages
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        private void SendLogs(object sender, string message)
        {
            Trace.WriteLine(message);
            _logBuilder.AppendLine(message);
        }

        /// <summary>
        /// Command listener saves the last OutCommand and error logs
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="cmd">The command.</param>
        private void SendCommands(object sender, OutCommand? cmd)
        {
            if (cmd == null)
            {
                _outCommand = null;
                return;
            }

            if (cmd.Command == -1 && !string.IsNullOrEmpty(cmd.ErrorMessage))
            {
                _logBuilder.AppendLine(cmd.ErrorMessage);
            }

            _outCommand = cmd;
        }


        /// <summary>
        /// Feedback event listener saves the event and logs user selection
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="IrtFeedbackInputEventArgs"/> instance containing the event data.</param>
        private void PromptHandleFeedback(object sender, IrtFeedbackInputEventArgs e)
        {
            Trace.WriteLine(e);
            _feedback = e;
            _logBuilder.AppendLine($"You selected:  {e.Answer.ToString().ToLower()}");
        }


        /// <summary>
        /// Helper to get accumulated log as string
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        private string Log => _logBuilder.ToString();

        /// <summary>
        /// Feedback of the extension.
        /// </summary>
        [TestMethod]
        public void FeedbackExtension()
        {
            var baseCommands = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 2, Description = "Help com1" } },
                { 1, new InCommand { Command = "com2", ParameterCount = 0, Description = "com2 Command Namespace 1" } },
                { 2, new InCommand { Command = "com3", ParameterCount = 0, Description = "Special case no Parameter" } }
            };

            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.Initiate(baseCommands, "UserSpace 1");

            prompt.ConsoleInput("coM1(1,2).Help()");

            Assert.IsTrue(Log.Contains("com1 Description"), "No help provided.");
            prompt.ConsoleInput("");
            Assert.IsTrue(Log.Contains("Input was not valid."), "Error was not caught.");

            prompt.ConsoleInput("mehh");
            Assert.IsTrue(Log.Contains("Input was not valid."), "Error was not caught.");

            prompt.ConsoleInput(" yeS   ");
            Assert.IsNotNull(_outCommand, "OutCommand should not be null.");

            prompt.ConsoleInput("List().Help()");
            Assert.IsTrue(Log.Contains("You now have the following Options:"), "Wrong options provided.");

            prompt.ConsoleInput("YeS   ");
            Assert.IsTrue(Log.Contains("Special case"), "Wrong commands listed.");
        }

        /// <summary>
        /// Tries the run extension with invalid base command should fail.
        /// </summary>
        [TestMethod]
        public void TryRunExtensionWithInvalidBaseCommandShouldFail()
        {
            var baseCommands = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 1, Description = "Help com1" } },
                { 1, new InCommand { Command = "com2", ParameterCount = 2, Description = "com2 Command Namespace 1" } },
                { 2, new InCommand { Command = "com3", ParameterCount = 0, Description = "Special case no Parameter" } }
            };

            var extensionCommands = new Dictionary<int, InCommand>
            {
                { 0, new InCommand
                    {
                        Command = "tryrun",
                        ParameterCount = 0,
                        FeedbackId = 1,
                        Description = "Show results and optionally run commands"
                    }
                }
            };

            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.HandleFeedback += PromptHandleFeedback;
            prompt.Initiate(baseCommands, "UserSpace 1", extensionCommands, Feedback);

            prompt.ConsoleInput("nope(1).tryrun()");

            Assert.IsTrue(Log.Contains("error KeyWord not Found:"), "Expected invalid input for bad base command.");
        }

        /// <summary>
        /// Confirms the internal test.
        /// </summary>
        [TestMethod]
        public void ConfirmInternalTest()
        {
            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.HandleFeedback += PromptHandleFeedback;
            prompt.Initiate(userFeedback: Feedback);

            prompt.ConsoleInput("confirm()");
            Assert.IsTrue(Log.Contains("You now have the following Options"), "Feedback options not provided on first confirm.");

            prompt.ConsoleInput("mehh");
            Assert.IsTrue(Log.Contains("Input was not valid."), "Invalid input not logged correctly.");

            prompt.ConsoleInput("yes");
            Assert.IsTrue(Log.Contains("You selected:  yes"), "Confirmation message missing after 'yes'.");
            Assert.IsNotNull(_feedback, "Feedback event not raised after 'yes'.");
            Assert.AreEqual(AvailableFeedback.Yes, _feedback.Answer, "Expected 'Yes' answer after valid input.");

            prompt.ConsoleInput("confirm(1)");
            Assert.IsTrue(Log.Contains("Do you want to commit the following changes?"), "Feedback options not provided on confirm(1).");
            Assert.IsNotNull(_feedback, "Feedback event not raised on confirm(1).");

            prompt.ConsoleInput("mehh");
            Assert.IsTrue(Log.Contains("Input was not valid."), "Invalid input not logged correctly on confirm(1).");

            prompt.ConsoleInput("yes");
            Assert.IsTrue(Log.Contains("You selected:  yes"), "Confirmation message missing after 'yes' on confirm(1).");
            Assert.IsNotNull(_feedback, "Feedback event not raised after 'yes' on confirm(1).");
            Assert.AreEqual(AvailableFeedback.Yes, _feedback.Answer, "Expected 'Yes' answer after valid input on confirm(1).");
        }
    }
}

