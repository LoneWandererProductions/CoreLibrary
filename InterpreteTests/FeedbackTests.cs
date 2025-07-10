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
using Interpreter.Resources;
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
        ///     The replace feedback
        ///     ORder does matter.
        /// </summary>
        private static readonly UserFeedback ReplaceFeedback = new()
        {
            Before = true,
            Message = "Do you want to commit the following changes?",
            Options = new Dictionary<AvailableFeedback, string>
            {
                { AvailableFeedback.Yes, "If you want to execute the Command type yes" },
                { AvailableFeedback.No, " If you want to stop executing the Command type no." }
            }
        };

        /// <summary>
        ///     For commands that need your feedback
        /// </summary>
        internal static readonly Dictionary<int, UserFeedback> Feedback = new() { { 1, ReplaceFeedback } };

        /// <summary>
        ///     The feedback
        /// </summary>
        private IrtFeedbackInputEventArgs _feedback;

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
        ///     Feedback and extension external test.
        /// </summary>
        [TestMethod]
        public void FeedbackExtensionExternal()
        {
            // Arrange
            var dctCommandOne = new Dictionary<int, InCommand>
            {
                { 0, new InCommand { Command = "com1", ParameterCount = 1, Description = "Help com1" } },
                {
                    1,
                    new InCommand { Command = "com2", ParameterCount = 2, Description = "com2 Command Namespace 1" }
                },
                {
                    2,
                    new InCommand
                    {
                        Command = "com3", ParameterCount = 0, Description = "Special case no Parameter"
                    }
                }
            };

            Dictionary<int, InCommand> extensionCommands = new()
            {
                {
                    0, new InCommand
                    {
                        Command = "tryrun",
                        ParameterCount = 0,
                        FeedbackId = 1,
                        Description =
                            "Show results and optional run commands"
                    }
                }
            };

            // Act
            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.Initiate(dctCommandOne, "UserSpace 1", extensionCommands, Feedback);
            prompt.ConsoleInput("coM2(1,2).tryrun()");
            prompt.ConsoleInput("");
        }

        /// <summary>
        ///     Feedback and extension external test.
        /// </summary>
        [TestMethod]
        public void ConfirmInternalTest()
        {
            // Arrange
            var prompt = new Prompt();
            prompt.SendLogs += SendLogs;
            prompt.SendCommands += SendCommands;
            prompt.HandleFeedback += PromptHandleFeedback;
            prompt.Initiate(userFeedback: Feedback);

            // Act & Assert

            // First confirm() triggers feedback request
            prompt.ConsoleInput("confirm()");
            Assert.IsTrue(_log.Contains("You now have the following Options"),
                "Feedback options not provided on first confirm.");

            // Invalid input: "mehh" -> log input invalid
            prompt.ConsoleInput("mehh");
            Assert.AreEqual("Input was not valid.", _log, "Invalid input not logged correctly.");

            // Valid input: "yes" -> feedback accepted, feedback reset
            prompt.ConsoleInput("yes");
            Assert.IsTrue(_log.Contains("You selected:  yes"), "Confirmation message missing after 'yes'.");
            Assert.IsNotNull(_feedback, "Feedback event not raised after 'yes'.");
            Assert.AreEqual(AvailableFeedback.Yes, _feedback.Answer, "Expected 'Yes' answer after valid input.");

            // Confirm with argument: confirm(1)
            prompt.ConsoleInput("confirm(1)");
            Assert.IsTrue(
                _log.Contains(
                    "Do you want to commit the following changes? Yes If you want to execute the Command type yes, No  If you want to stop executing the Command type no"),
                "Feedback options not provided on confirm(1).");
            Assert.IsNotNull(_feedback, "Feedback event not raised on confirm(1).");

            // Invalid input again
            prompt.ConsoleInput("mehh");
            Assert.AreEqual("Input was not valid.", _log, "Invalid input not logged correctly on confirm(1).");

            // Valid input again
            prompt.ConsoleInput("yes");
            Assert.IsTrue(_log.Contains("You selected:  yes"),
                "Confirmation message missing after 'yes' on confirm(1).");
            Assert.IsNotNull(_feedback, "Feedback event not raised after 'yes' on confirm(1).");
            Assert.AreEqual(AvailableFeedback.Yes, _feedback.Answer,
                "Expected 'Yes' answer after valid input on confirm(1).");
        }

        /// <summary>
        ///     Handles the HandleFeedback event of the Prompt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="IrtFeedbackInputEventArgs" /> instance containing the event data.</param>
        private void PromptHandleFeedback(object sender, IrtFeedbackInputEventArgs e)
        {
            Trace.WriteLine(e);
            _feedback = e;
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
