/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtPrompt.cs
 * PURPOSE:     Handle the Input
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Interpreter
{
    /// <summary>
    ///     Basic command Line Interpreter, bare bones for now
    /// </summary>
    internal sealed class IrtPrompt
    {
        /// <summary>
        ///     Command Register
        /// </summary>
        private static Dictionary<int, InCommand> _com;

        /// <summary>
        ///     Namespace of Commands
        /// </summary>
        private static string _nameSpace;

        /// <summary>
        ///     Send selected Command to the Subscriber
        /// </summary>
        internal EventHandler<OutCommand> sendCommand;

        /// <summary>
        ///     Send selected Command to the Subscriber
        /// </summary>
        internal EventHandler<string> sendLog;

        /// <summary>Get the Engine Running</summary>
        /// <param name="use">The Command Structure</param>
        internal void Initiate(UserSpace use)
        {
            _com = use.Commands;
            _nameSpace = use.UserSpaceName;
            var log = ErrorLogging.SetLastError(IrtConst.InformationStartup, 2);
            OnStatus(log);
        }

        /// <summary>
        ///     will do all the work
        /// </summary>
        /// <param name="inputString">Input string</param>
        /// <returns>Results of our commands</returns>
        internal void HandleInput(string inputString)
        {
            //clean string, remove trailing whitespace
            inputString = inputString.Trim().ToUpper(CultureInfo.CurrentCulture).ToUpper(CultureInfo.InvariantCulture);

            //handle File comments
            if (inputString.StartsWith(IrtConst.CommentCommand))
            {
                Trace.WriteLine(inputString);
                return;
            }

            //Help definition
            if (inputString.Equals(IrtConst.InternalCommandHelp, StringComparison.InvariantCultureIgnoreCase))
            {
                OnStatus(IrtConst.HelpGeneric);
                return;
            }

            //check if it is an internal Command
            var param = Irt.CheckInternalCommands(inputString);

            if (!string.IsNullOrEmpty(param))
            {
                HandleInternalCommands(param, inputString);
                return;
            }

            //check if Command Dictionary was empty
            if (_com == null)
            {
                OnStatus(IrtConst.ErrorNoCommandsProvided);
                SetError();
                return;
            }

            //Get the Id of the Command Dictionary
            var key = Irt.CheckForKeyWord(inputString, _com);

            //if key was not found bail
            if (key == IrtConst.ErrorParam)
            {
                var log = ErrorLogging.SetLastError(IrtConst.KeyWordNotFoundError, 0);
                OnStatus(string.Concat(log, inputString));
                SetError();
                return;
            }

            //Is Parameter count > 0 and parentheses correct?
            if (_com[key].ParameterCount != 0 && !Irt.SingleCheck(inputString))
            {
                var log = ErrorLogging.SetLastError(IrtConst.ParenthesisError, 0);
                OnStatus(log);
                SetError();
                return;
            }

            var parameterPart = Irt.RemoveWord(_com[key].Command.ToUpper(CultureInfo.InvariantCulture), inputString);
            //Remove Parenthesis and split
            parameterPart = Irt.RemoveParenthesis(parameterPart, IrtConst.BaseClose, IrtConst.BaseOpen);
            var parameter = Irt.SplitParameter(parameterPart, IrtConst.Splitter);

            //Check for overloads, get the Overload by Parameter Count, return -1 if we do not find a match
            key = Irt.CheckOverload(_com[key].Command, parameter.Count, _com);

            //Incorrect overload
            if (key == IrtConst.ErrorParam)
            {
                var log = ErrorLogging.SetLastError(IrtConst.SyntaxError, 0);
                OnStatus(log);
                SetError();
                return;
            }

            SetResult(key, parameter);
        }

        /// <summary>
        ///     For Internal Commands
        /// </summary>
        /// <param name="param">Parameter of the internal Command.</param>
        /// <param name="inputString">Input string</param>
        private void HandleInternalCommands(string param, string inputString)
        {
            //sure the beginning is like the Internal command but with some extras we do not like
            if (param == IrtConst.InternalCommandList && param != inputString)
            {
                var log = ErrorLogging.SetLastError(IrtConst.SyntaxError, 0);
                OnStatus(log);
                SetError();
                return;
            }

            var parameterPart = Irt.RemoveWord(param.ToUpper(CultureInfo.InvariantCulture), inputString);

            switch (param)
            {
                case IrtConst.InternalCommandHelp:
                    CommandHelp(parameterPart);
                    break;

                case IrtConst.InternalCommandList:
                    CommandList();
                    break;

                case IrtConst.InternalUsing:
                    CommandUsing(_nameSpace);
                    break;

                case IrtConst.InternalUse:
                    CommandUse(parameterPart);
                    break;

                case IrtConst.InternalCommandContainer:
                    CommandContainer(inputString, parameterPart);
                    break;
                case IrtConst.InternalCommandBatchExecute:
                    CommandBatchExecute(parameterPart);
                    break;
                default:
                    OnStatus(string.Concat(IrtConst.KeyWordNotFoundError, param));
                    break;
            }
        }

        /// <summary>
        ///     Return help for specific command
        /// </summary>
        /// <param name="parameterPart">Parameter about what we want help about</param>
        private void CommandHelp(string parameterPart)
        {
            //Empty parameters
            if (parameterPart == "()")
            {
                OnStatus(IrtConst.HelpGeneric);
                return;
            }

            //Remove Parenthesis
            parameterPart = Irt.RemoveParenthesis(parameterPart, IrtConst.BaseClose, IrtConst.BaseOpen);

            //Get the Id
            var key = Irt.CheckForKeyWord(parameterPart, _com);

            //anything else error out
            if (key == IrtConst.ErrorParam)
            {
                var log = ErrorLogging.SetLastError(string.Concat(IrtConst.KeyWordNotFoundError, parameterPart), 0);
                OnStatus(log);
                SetError();
                return;
            }

            OnStatus(string.Concat(_com[key].Command, IrtConst.FormatDescription, _com[key].Description,
                IrtConst.FormatCount,
                _com[key].ParameterCount));
        }

        /// <summary>Command to switch between using.</summary>
        /// <param name="parameterPart">The parameter part.</param>
        private void CommandUse(string parameterPart)
        {
            //Remove Parenthesis and split
            parameterPart = Irt.RemoveParenthesis(parameterPart, IrtConst.BaseClose, IrtConst.BaseOpen);

            if (!Prompt.CollectedSpaces.ContainsKey(parameterPart))
            {
                OnStatus(IrtConst.ErrorUserSpaceNotFound);
                return;
            }

            Prompt.SwitchNameSpaces(parameterPart);
        }

        /// <summary>
        ///     List all available Commands
        /// </summary>
        private void CommandList()
        {
            foreach (var com in _com.Values)
            {
                OnStatus(string.Concat(com.Command, Environment.NewLine));
            }
        }

        /// <summary>Display all using and the Current in Use.</summary>
        /// <param name="nameSpace">The name space.</param>
        private void CommandUsing(string nameSpace)
        {
            OnStatus(string.Concat(IrtConst.Active, nameSpace, Environment.NewLine));
            foreach (var key in Prompt.CollectedSpaces.Keys)
            {
                OnStatus(string.Concat(key, Environment.NewLine));
            }
        }

        /// <summary>
        ///     Processes the container.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="parameterPart">Parameter Part.</param>
        private void CommandContainer(string inputString, string parameterPart)
        {
            char[] openParenthesis = { IrtConst.BaseOpen, IrtConst.AdvancedOpen };
            char[] closeParenthesis = { IrtConst.BaseClose, IrtConst.AdvancedClose };

            var check = Irt.CheckMultiple(inputString, openParenthesis, closeParenthesis);

            //Remove outer and first Parenthesis
            parameterPart = Irt.RemoveLastOccurrence(parameterPart, IrtConst.AdvancedClose);
            parameterPart = Irt.RemoveFirstOccurrence(parameterPart, IrtConst.AdvancedOpen);

            if (!check)
            {
                {
                    var log = ErrorLogging.SetLastError(IrtConst.ParenthesisError, 0);
                    OnStatus(log);
                    SetError();
                }
            }

            GenerateCommands(parameterPart);
        }

        /// <summary>
        ///     Batch execute of Commands in a file.
        /// </summary>
        /// <param name="parameterPart">The Parameter.</param>
        private void CommandBatchExecute(string parameterPart)
        {
            //Remove Parenthesis
            parameterPart = Irt.RemoveParenthesis(parameterPart, IrtConst.BaseClose, IrtConst.BaseOpen);
            parameterPart = IrtHelper.ReadBatchFile(parameterPart);

            //check if Command Dictionary was empty
            if (parameterPart?.Length == 0)
            {
                OnStatus(IrtConst.ErrorFileNotFound);
                SetError();
                return;
            }

            GenerateCommands(parameterPart);
        }

        /// <summary>
        ///     Generates the commands.
        /// </summary>
        /// <param name="parameterPart">The parameter part.</param>
        private void GenerateCommands(string parameterPart)
        {
            foreach (var com in Irt.SplitParameter(parameterPart, IrtConst.NewCommand))
            {
                HandleInput(com);
            }
        }

        /// <summary>
        ///     Decide the appropriate Action by command
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameter">List of Parameters</param>
        /// <returns>Result of our Command</returns>
        private void SetResult(int key, List<string> parameter)
        {
            var com = new OutCommand { Command = key, Parameter = parameter };

            OnCommand(com);
        }

        /// <summary>
        ///     Set the error Status of the Output command
        /// </summary>
        private void SetError()
        {
            var com = new OutCommand { Command = IrtConst.ErrorParam, Parameter = null };

            OnCommand(com);
        }

        /// <summary>
        ///     Sends everything
        /// </summary>
        /// <param name="sendLog">Debug and Status Messages</param>
        private void OnStatus(string sendLog)
        {
            this.sendLog?.Invoke(this, sendLog);
        }

        /// <summary>
        ///     Only sends Commands
        /// </summary>
        /// <param name="outCommand">Selected User Command</param>
        private void OnCommand(OutCommand outCommand)
        {
            sendCommand?.Invoke(this, outCommand);
        }
    }
}
