/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtParserInput.cs
 * PURPOSE:     Handle the Input for prompt and connect to the other modules
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Local

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Interpreter.Resources;

namespace Interpreter
{
    /// <inheritdoc />
    /// <summary>
    ///     Basic command Line Interpreter, bare bones for now
    /// </summary>
    internal sealed class IrtParserInput : IDisposable
    {
        /// <summary>
        ///     Command Register
        /// </summary>
        private static Dictionary<int, InCommand>? _com;

        /// <summary>
        ///     Namespace of Commands
        /// </summary>
        private static string? _nameSpace;

        /// <summary>
        ///     Extension Command Register
        /// </summary>
        private static Dictionary<int, InCommand>? _extension;

        /// <summary>
        ///     My request identifier
        /// </summary>
        private readonly string _myRequestId;

        /// <summary>
        ///     The prompt
        /// </summary>
        private readonly Prompt _prompt;

        /// <summary>
        ///     The user feedback
        /// </summary>
        private readonly Dictionary<int, UserFeedback>? _userFeedback;

        /// <summary>
        ///     The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     The original input string
        /// </summary>
        private string _inputString;

        /// <summary>
        ///     The IRT extension
        /// </summary>
        private IrtExtension _irtExtension;

        /// <summary>
        ///     The irt handle extension internal
        /// </summary>
        private IrtHandleExtensionInternal _irtHandleExtensionInternal;

        /// <summary>
        ///     The IRT internal
        /// </summary>
        private IrtHandleInternal _irtHandleInternal;

        /// <summary>
        ///     Prevents a default instance of the <see cref="IrtParserInput" /> class from being created.
        /// </summary>
        private IrtParserInput()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrtParserInput" /> class.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="userFeedback">The optional user feedback.</param>
        public IrtParserInput(Prompt prompt, Dictionary<int, UserFeedback>? userFeedback = null)
        {
            _prompt = prompt;
            _myRequestId = Guid.NewGuid().ToString();
            _userFeedback = userFeedback;
            _prompt.HandleFeedback += HandleFeedback;
        }


        /// <inheritdoc />
        /// <summary>
        ///     Dispose of the resources used by the IrtPrompt.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Event to send selected command to the subscriber
        /// </summary>
        internal event EventHandler<string> SendInternalLog;

        /// <summary>
        ///     Get the engine running
        /// </summary>
        /// <param name="use">The command structure</param>
        internal void Initiate(UserSpace use)
        {
            // Set the basic parameters
            _com = use.Commands;
            _extension = use.ExtensionCommands;
            _nameSpace = use.UserSpaceName;
            _irtExtension = new IrtExtension();
            _irtHandleInternal = new IrtHandleInternal(use.Commands, use.UserSpaceName, _prompt, _userFeedback);
            _irtHandleExtensionInternal =
                new IrtHandleExtensionInternal(this, use.Commands, _prompt, _irtHandleInternal, _userFeedback);

            // Notify log about loading up
            var log = Logging.SetLastError(IrtConst.InformationStartup, 2);
            SendInternalLog?.Invoke(this, log);
        }

        /// <summary>
        ///     Switches the user space.
        /// </summary>
        /// <param name="use">The use.</param>
        internal static void SwitchUserSpace(UserSpace use)
        {
            _com = use.Commands;
            _extension = use.ExtensionCommands;
            _nameSpace = use.UserSpaceName;
        }

        /// <summary>
        ///     Handles the input string and processes commands.
        /// </summary>
        /// <param name="inputString">Input string</param>
        internal void HandleInput(string inputString)
        {
            _inputString = inputString;

            if (string.IsNullOrWhiteSpace(inputString))
            {
                SetError(IrtConst.ErrorInvalidInput);
                return;
            }

            // Validate the input string
            if (!CleanAndCheckInputString(ref inputString))
            {
                SetError(Logging.SetLastError(IrtConst.ParenthesisError, 0));
                return;
            }

            // Handle comment commands
            if (IsCommentCommand(inputString))
            {
                Trace.WriteLine(inputString);
                return;
            }

            // Handle help commands
            if (IsHelpCommand(inputString))
            {
                _prompt.SendLog(this, IrtConst.HelpGeneric);
                return;
            }

            // Check if the parentheses are correct
            if (!IrtKernel.SingleCheck(inputString))
            {
                SetErrorWithLog(IrtConst.ParenthesisError);
                return;
            }

            // Check for extensions in the internal namespace first, then in the external namespace if needed
            var extensionResult = _irtExtension.CheckForExtension(_inputString, IrtConst.InternalNameSpace,
                IrtConst.InternalExtensionCommands);

            if (extensionResult.Status == IrtConst.Error)
            {
                extensionResult = _irtExtension.CheckForExtension(_inputString, _nameSpace, _extension);
            }

            // Process the extension result
            switch (extensionResult.Status)
            {
                case IrtConst.Error:
                    SetError(Logging.SetLastError($"{IrtConst.ErrorExtensions}{IrtConst.Error}", 0));
                    return;

                case IrtConst.ParameterMismatch:
                    SetError(Logging.SetLastError($"{IrtConst.ErrorExtensions}{IrtConst.ParameterMismatch}", 0));
                    return;

                case IrtConst.ExtensionFound:
                    if (extensionResult.Extension.ExtensionNameSpace == IrtConst.InternalNameSpace)
                    {
                        _irtHandleExtensionInternal.ProcessExtensionInternal(extensionResult.Extension);
                    }
                    else
                    {
                        //TODO result is wrong.
                        var command = ProcessInput(inputString, extensionResult.Extension);
                        SetResult(command);
                    }

                    return;

                default:
                    var com = ProcessInput(inputString);
                    if (com != null)
                    {
                        SetResult(com);
                    }

                    break;
            }
        }

        /// <summary>
        ///     Processes the input string.
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <param name="extension">Optional extension commands</param>
        internal OutCommand? ProcessInput(string inputString, ExtensionCommands? extension = null)
        {
            if (extension != null)
            {
                //remove the extension string, and handle only the base command.
                inputString = extension.BaseCommand;
            }

            var key = IrtKernel.CheckForKeyWord(inputString, IrtConst.InternCommands);
            if (key != IrtConst.Error)
            {
                _irtHandleInternal.ProcessInput(key, inputString);
                return null;
            }

            key = IrtKernel.CheckForKeyWord(inputString, _com);
            if (key == IrtConst.Error)
            {
                SetErrorWithLog(IrtConst.KeyWordNotFoundError, _inputString);
                return null;
            }

            var (status, splitParameter) = IrtKernel.GetParameters(inputString, key, _com);
            var parameter = status == IrtConst.ParameterCommand
                ? IrtKernel.SplitParameter(splitParameter, IrtConst.Splitter)
                : new List<string> { splitParameter };

            var check = IrtKernel.CheckOverload(_com[key].Command, parameter.Count, _com);

            if (check != null)
            {
                return new OutCommand
                {
                    Command = (int)check,
                    Parameter = parameter,
                    UsedNameSpace = _nameSpace,
                    ExtensionCommand = extension
                };
            }

            SetErrorWithLog(IrtConst.SyntaxErrorParameterCount);

            return null;
        }

        /// <summary>
        ///     Cleans the input string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>True if input string is cleaned and valid; otherwise false.</returns>
        private static bool CleanAndCheckInputString(ref string input)
        {
            input = IrtKernel.WellFormedParenthesis(input);
            var openParenthesis = new[] { IrtConst.BaseOpen, IrtConst.AdvancedOpen };
            var closeParenthesis = new[] { IrtConst.BaseClose, IrtConst.AdvancedClose };

            return IrtKernel.CheckMultipleParenthesis(input, openParenthesis, closeParenthesis);
        }

        /// <summary>
        ///     Determines whether the specified input is a comment command.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///     <c>true</c> if the specified input is a comment command; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCommentCommand(string input)
        {
            return input.StartsWith(IrtConst.CommentCommand, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Determines whether the specified input is a help command.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///     <c>true</c> if the specified input is a help command; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsHelpCommand(string input)
        {
            input = input.Replace(IrtConst.BaseOpen.ToString(), string.Empty)
                .Replace(IrtConst.BaseClose.ToString(), string.Empty);
            return input.Equals(IrtConst.InternalCommandHelp, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Sets the error with log.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="additionalInfo">Additional information.</param>
        private void SetErrorWithLog(string errorMessage, string additionalInfo = "")
        {
            var log = Logging.SetLastError(errorMessage, 0);
            SetError(string.Concat(log, additionalInfo));
        }

        /// <summary>
        ///     Sets the result of a command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void SetResult(OutCommand command)
        {
            if (_com[command.Command]?.FeedbackId == 0)
            {
                _prompt.SendCommand(this, command);
            }
            else
            {
                if (_userFeedback == null)
                {
                    SetErrorWithLog(IrtConst.ErrorNoFeedbackOptions);
                    return;
                }

                var id = _com[command.Command].FeedbackId;


                //Todo add access to the Internal Feedback as well
                var feedback = _userFeedback[id];
                var feedbackReceiver = new IrtFeedback
                {
                    RequestId = _myRequestId, Feedback = feedback, AwaitedOutput = command
                };

                _prompt.RequestFeedback(feedbackReceiver);
            }
        }

        /// <summary>
        ///     Handles the feedback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="IrtFeedbackInputEventArgs" /> instance containing the event data.</param>
        private void HandleFeedback(object? sender, IrtFeedbackInputEventArgs e)
        {
            if (e.RequestId != _myRequestId)
            {
                return;
            }

            switch (e.Answer)
            {
                case AvailableFeedback.Yes:
                    _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedYes);
                    _prompt.SendCommand(this, e.AwaitedOutput);
                    break;
                case AvailableFeedback.No:
                    _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedNo);
                    break;
                case AvailableFeedback.Cancel:
                    _prompt.SendLog(this, IrtConst.FeedbackCancelOperation);
                    break;
            }
        }

        /// <summary>
        ///     Sets the error status of the output command.
        /// </summary>
        /// <param name="error">The error message.</param>
        private void SetError(string error)
        {
            var com = new OutCommand
            {
                Command = IrtConst.Error, Parameter = null, UsedNameSpace = _nameSpace, ErrorMessage = error
            };

            _prompt.SendCommand(this, com);
        }

        /// <summary>
        ///     Dispose the resources.
        /// </summary>
        /// <param name="disposing">
        ///     Indicates whether the method call comes from a Dispose method (true) or from a finalizer
        ///     (false).
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
                // Dispose managed resources here if needed
            {
                _irtHandleInternal?.Dispose();
            }

            // Dispose unmanaged resources here if needed

            _disposed = true;
        }

        /// <summary>
        ///     Destructor to ensure the resources are released.
        /// </summary>
        ~IrtParserInput()
        {
            Dispose(false);
        }
    }
}
