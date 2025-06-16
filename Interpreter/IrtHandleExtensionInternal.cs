/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtHandleExtensionInternal.cs
 * PURPOSE:     Handle the Input for Internal Extensions
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;

namespace Interpreter
{
    /// <inheritdoc />
    /// <summary>
    ///     Helper class for handling internal extensions.
    /// </summary>
    internal sealed class IrtHandleExtensionInternal : IDisposable
    {
        /// <summary>
        ///     My request identifier
        /// </summary>
        private readonly string _myRequestId;

        /// <summary>
        ///     Dictionary of available commands.
        /// </summary>
        private Dictionary<int, InCommand> _commands;

        /// <summary>
        ///     Indicates whether the object has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Instance of IrtHandleInternal for handling internal command logic.
        /// </summary>
        private IrtHandleInternal _irtHandleInternal;

        /// <summary>
        ///     Instance of IrtParser for internal use.
        /// </summary>
        private IrtParserInput _irtHandlePrompt;

        /// <summary>
        ///     Instance of Prompt to handle command input/output.
        /// </summary>
        private Prompt _prompt;

        /// <summary>
        ///     The user feedback, only for external extensions.
        /// </summary>
        private readonly Dictionary<int, UserFeedback> _userFeedback;

        /// <summary>
        ///     Prevents a default instance of the <see cref="IrtHandleExtensionInternal" /> class from being created.
        /// </summary>
        private IrtHandleExtensionInternal()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrtHandleExtensionInternal" /> class with specified parameters.
        /// </summary>
        /// <param name="irtPrompt">Instance of IrtParser.</param>
        /// <param name="commands">Dictionary of commands.</param>
        /// <param name="prompt">Instance of Prompt.</param>
        /// <param name="irtInternal">Instance of IrtHandleInternal.</param>
        /// <param name="userFeedback">The user feedback.</param>
        public IrtHandleExtensionInternal(IrtParserInput irtPrompt, Dictionary<int, InCommand> commands, Prompt prompt,
            IrtHandleInternal irtInternal, Dictionary<int, UserFeedback> userFeedback = null)
        {
            _irtHandlePrompt = irtPrompt;
            _commands = commands;
            _prompt = prompt;
            _userFeedback = userFeedback;
            _prompt.HandleFeedback += HandleFeedback;
            _myRequestId = Guid.NewGuid().ToString();
            _irtHandleInternal = irtInternal;
            _disposed = false;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Releases all resources used by the <see cref="T:Interpreter.IrtHandleExtensionInternal" /> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Processes the internal extension based on the given extension command.
        /// </summary>
        /// <param name="extension">The extension command to process.</param>
        internal void ProcessExtensionInternal(ExtensionCommands extension)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(IrtHandleExtensionInternal));
            }

            switch (extension.ExtensionCommand)
            {
                case 0:
                    // Switch namespace
                    _prompt.SwitchNameSpaces(extension.ExtensionParameter[0]);
                    _prompt.ConsoleInput(extension.BaseCommand);
                    break;
                case 1:
                    ProcessHelpCommand(extension);
                    break;
                default:
                    _prompt.SendLogs(nameof(ProcessExtensionInternal), IrtConst.ErrorInternalExtensionNotFound);
                    break;
            }
        }

        /// <summary>
        ///     Processes help command and requests feedback.
        /// </summary>
        /// <param name="extension">The extension command to process.</param>
        private void ProcessHelpCommand(ExtensionCommands extension)
        {
            var key = IrtKernel.CheckForKeyWord(extension.BaseCommand, IrtConst.InternCommands);

            if (key != IrtConst.Error)
            {
                HandleInternalCommand(extension, key);
            }
            else
            {
                HandleExternalCommand(extension);
            }
        }

        /// <summary>
        ///     Handles internal commands.
        /// </summary>
        /// <param name="extension">The extension command.</param>
        /// <param name="key">The command key.</param>
        private void HandleInternalCommand(ExtensionCommands extension, int key)
        {
            var command = IrtConst.InternCommands[key];

            using (var irtInternal =
                   new IrtHandleInternal(IrtConst.InternCommands, IrtConst.InternalNameSpace, _prompt))
            {
                irtInternal.ProcessInput(IrtConst.InternalHelpWithParameter, command.Command);
            }

            UserFeedback feedback = null;

            //if none is available use the standard one from Internal
            //if not we use the provided one
            if (_userFeedback?.TryGetValue(extension.FeedBackId, out feedback) != true)
            {
                feedback = IrtConst.InternalFeedback[-1];
            }

            var feedbackReceiver = new IrtFeedback
            {
                RequestId = _myRequestId,
                Feedback = feedback,
                BranchId = 11,
                Key = key,
                Command = extension.BaseCommand
            };

            _prompt.RequestFeedback(feedbackReceiver);
        }

        /// <summary>
        /// Handles external commands.
        /// </summary>
        /// <param name="extension">The extension command.</param>
        private void HandleExternalCommand(ExtensionCommands extension)
        {
            var com = _irtHandlePrompt.ProcessInput(extension.BaseCommand);
            var command = _commands[com.Command];
            _irtHandleInternal.ProcessInput(IrtConst.InternalHelpWithParameter, command.Command);

            //TODO completly wrong here
            var feedback = IrtConst.InternalFeedback[-1];

            var feedbackReceiver = new IrtFeedback
            {
                RequestId = _myRequestId, Feedback = feedback, BranchId = 12, AwaitedOutput = com
            };

            _prompt.RequestFeedback(feedbackReceiver);
        }

        /// <summary>
        ///     Handles the feedback for ProcessExtensionInternal.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="IrtFeedbackInputEventArgs" /> instance containing the event data.</param>
        private void HandleFeedback(object sender, IrtFeedbackInputEventArgs e)
        {
            if (e.RequestId != _myRequestId)
            {
                return;
            }

            switch (e.BranchId)
            {
                case 11:
                    switch (e.Answer)
                    {
                        case AvailableFeedback.Yes:
                            _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedYes);
                            _irtHandleInternal.ProcessInput(e.Key, e.Command);
                            break;
                        case AvailableFeedback.No:
                            _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedNo);
                            break;
                        case AvailableFeedback.Cancel:
                            _prompt.SendLog(this, IrtConst.FeedbackCancelOperation);
                            break;
                    }

                    break;
                case 12:
                    switch (e.Answer)
                    {
                        case AvailableFeedback.Yes:
                            _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedYes);
                            _prompt.SendCommands(this, e.AwaitedOutput);
                            break;
                        case AvailableFeedback.No:
                            _prompt.SendLog(this, IrtConst.FeedbackOperationExecutedNo);
                            break;
                        case AvailableFeedback.Cancel:
                            _prompt.SendLog(this, IrtConst.FeedbackCancelOperation);
                            break;
                    }

                    break;
            }
        }

        /// <summary>
        ///     Releases the unmanaged resources and optionally releases the managed resources.
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
            {
                _prompt.HandleFeedback -= HandleFeedback;
                _commands = null;
                _irtHandlePrompt = null;
                _prompt = null;
                _irtHandleInternal = null;
            }

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="IrtHandleExtensionInternal" /> class.
        /// </summary>
        ~IrtHandleExtensionInternal()
        {
            Dispose(false);
        }
    }
}
