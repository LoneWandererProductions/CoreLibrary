/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtHandleInternal.cs
 * PURPOSE:     Handle the Input for all Internal commands. Must be mire clean since it will handle all batch and container Commands.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace Interpreter
{
    /// <inheritdoc />
    /// <summary>
    ///     Instance to handle all internal Commands
    /// </summary>
    internal sealed class IrtHandleInternal : IDisposable
    {
        /// <summary>
        ///     The name space
        /// </summary>
        private static string _nameSpace;

        /// <summary>
        ///     The prompt
        /// </summary>
        private readonly Prompt _prompt;

        /// <summary>
        ///     The commands
        /// </summary>
        private Dictionary<int, InCommand> _commands;

        /// <summary>
        ///     Indicates whether the object has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IrtHandleInternal" /> class.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="prompt">The prompt</param>
        internal IrtHandleInternal(Dictionary<int, InCommand> commands, string nameSpace, Prompt prompt)
        {
            _commands = commands;
            _nameSpace = nameSpace;
            _prompt = prompt;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Processes the input.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="inputString">The input string.</param>
        internal void ProcessInput(int key, string inputString)
        {
            var (status, parts) = IrtKernel.GetParameters(inputString, key, IrtConst.InternCommands);

            //handle normal command and batch/containers
            var parameter = status == IrtConst.ParameterCommand
                ? IrtKernel.SplitParameter(parts, IrtConst.Splitter)
                : new List<string> { parts };

            //check for batch and Command Files
            if (key is IrtConst.InternalContainerId or IrtConst.InternalBatchId)
            {
                HandleInternalCommands(key, parameter);
                return;
            }

            //check for Parameter Overload
            var check = IrtKernel.CheckOverload(IrtConst.InternCommands[key].Command, parameter.Count,
                IrtConst.InternCommands);

            if (check == null)
            {
                _prompt.SendLog(this, IrtConst.SyntaxError);
                return;
            }

            HandleInternalCommands(check, parameter);
        }

        /// <summary>
        ///     For Internal Commands
        /// </summary>
        /// <param name="command">Key of the command</param>
        /// <param name="parameter">optional parameters, can be empty or null.</param>
        private void HandleInternalCommands(int? command, IReadOnlyList<string> parameter)
        {
            switch (command)
            {
                case 1:
                    CommandHelp(parameter[0]);
                    break;

                case 2:
                    CommandList();
                    break;

                case 3:
                    CommandUsing(_nameSpace);
                    break;

                case 4:
                    //all using are in Uppercase, so handle this here.
                    CommandUse(parameter[0].ToUpperInvariant());
                    break;

                case 5:
                    CommandLogInfo();
                    break;

                case 6:
                    CommandLogError();
                    break;

                case 7:
                    CommandLogFull();
                    break;

                case 8:
                    using (var irtContainer = new IrtHandleContainer(this, _prompt))
                    {
                        irtContainer.CommandContainer(parameter[0]);
                    }

                    break;

                case 9:
                    using (var irtContainer = new IrtHandleContainer(this, _prompt))
                    {
                        irtContainer.CommandBatchExecute(parameter[0]);
                    }

                    break;

                case 10:
                    CommandPrint(parameter[0]);
                    break;

                default:
                    OnStatus(string.Concat(IrtConst.KeyWordNotFoundError, command));
                    break;
            }
        }

        /// <summary>
        ///     Return help for specific command
        /// </summary>
        /// <param name="parameterPart">Parameter about what we want help about</param>
        private void CommandHelp(string parameterPart)
        {
            if (parameterPart == IrtConst.InternalEmptyParameter)
            {
                OnStatus(IrtConst.HelpGeneric);
                return;
            }

            var key = IrtKernel.CheckForKeyWord(parameterPart, _commands);

            if (key == IrtConst.Error)
            {
                SetError(Logging.SetLastError($"{IrtConst.KeyWordNotFoundError}{parameterPart}", 0));
                return;
            }

            var commandInfo = _commands[key];
            OnStatus(
                $"{commandInfo.Command}{IrtConst.FormatDescription}{commandInfo.Description}{IrtConst.FormatCount}{commandInfo.ParameterCount}");
        }

        /// <summary>Command to switch between using.</summary>
        /// <param name="parameterPart">The parameter part.</param>
        private void CommandUse(string parameterPart)
        {
            parameterPart = IrtKernel.RemoveParenthesis(parameterPart, IrtConst.BaseOpen, IrtConst.BaseClose);

            if (!_prompt.CollectedSpaces.ContainsKey(parameterPart))
            {
                OnStatus(IrtConst.ErrorUserSpaceNotFound);
                return;
            }

            _prompt.SwitchNameSpaces(parameterPart);
            OnStatus($"{IrtConst.InformationNamespaceSwitch}{parameterPart}");
        }

        /// <summary>
        ///     List all available Commands
        /// </summary>
        private void CommandList()
        {
            foreach (var com in _commands.Values)
                OnStatus($"{com.Command}{Environment.NewLine}{com.Description}");
        }

        /// <summary>Display all using and the Current in Use.</summary>
        /// <param name="nameSpace">The name space.</param>
        private void CommandUsing(string nameSpace)
        {
            OnStatus($"{IrtConst.Active}{nameSpace}");
            foreach (var key in _prompt.CollectedSpaces.Keys)
                OnStatus(key);
        }

        /// <summary>
        ///     Commands the log.
        /// </summary>
        private void CommandLogError()
        {
            foreach (var entry in Logging.Log)
                OnStatus($"{entry}{Environment.NewLine}");
        }

        /// <summary>
        ///     Commands the log information.
        /// </summary>
        private void CommandLogInfo()
        {
            var message =
                $"{IrtConst.MessageLogStatistics}{Environment.NewLine}{IrtConst.MessageErrorCount}{Logging.Log.Count}{Environment.NewLine}{IrtConst.MessageLogCount}{_prompt.Log.Count}";
            OnStatus(message);
        }

        /// <summary>
        ///     Commands the log full.
        /// </summary>
        private void CommandLogFull()
        {
            foreach (var entry in new List<string>(_prompt.Log.Values))
                _prompt.SendLogs?.Invoke(nameof(IrtHandleInternal), entry);
        }

        /// <summary>
        ///     Commands the print.
        /// </summary>
        /// <param name="message">The message.</param>
        private void CommandPrint(string message)
        {
            _prompt.SendLogs?.Invoke(nameof(IrtHandleInternal), message);
        }

        /// <summary>
        ///     Sets the error.
        /// </summary>
        /// <param name="error">The error.</param>
        internal void SetError(string error)
        {
            var com = new OutCommand
                { Command = IrtConst.Error, Parameter = null, UsedNameSpace = _nameSpace, ErrorMessage = error };

            OnCommand(com);
        }

        /// <summary>
        ///     Only sends Commands
        /// </summary>
        /// <param name="outCommand">Selected User Command</param>
        private void OnCommand(OutCommand outCommand)
        {
            _prompt.SendCommands?.Invoke(this, outCommand);
        }

        /// <summary>
        ///     Sends everything
        /// </summary>
        /// <param name="sendLog">Debug and Status Messages</param>
        private void OnStatus(string sendLog)
        {
            _prompt.SendLogs?.Invoke(this, sendLog);
        }

        /// <summary>
        ///     Disposes of the resources used by the class.
        /// </summary>
        /// <param name="disposing">
        ///     Indicates whether the method call comes from a Dispose method (true) or from a finalizer
        ///     (false).
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                // Dispose managed resources here if needed
                // Set the local dictionary reference to null to remove references without clearing the dictionary
                _commands = null;

            // Dispose unmanaged resources here if needed

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="IrtHandleInternal" /> class.
        /// </summary>
        ~IrtHandleInternal()
        {
            Dispose(false);
        }
    }
}