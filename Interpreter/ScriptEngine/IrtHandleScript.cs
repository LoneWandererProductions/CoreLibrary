/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Interpreter.ScriptEngine/IrtHandleScript.cs
 * PURPOSE:     Handles all the stuff with container and batch files.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ExtendedSystemObjects;
using Interpreter.Resources;

namespace Interpreter.ScriptEngine
{
    /// <inheritdoc />
    /// <summary>
    ///     Handle our Container
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class IrtHandleScript : IDisposable
    {
        private bool _disposed;

        // Track nesting level of if blocks
        private int _ifDepth;
        private IrtHandleInternal _irtHandleInternal;

        // Persistent storage for parsed and categorized commands
        private readonly CategorizedDictionary<int, string> _parsedCommands = new();
        private Prompt _prompt;

        private IrtHandleScript()
        {
        }

        internal IrtHandleScript(IrtHandleInternal irtHandleInternal, Prompt prompt)
        {
            _irtHandleInternal = irtHandleInternal;
            _prompt = prompt;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void CommandContainer(string parameterPart)
        {
            parameterPart = IrtKernel.CutLastOccurrence(parameterPart, IrtConst.AdvancedClose);
            parameterPart = IrtKernel.RemoveFirstOccurrence(parameterPart, IrtConst.AdvancedOpen);

            GenerateCommands(parameterPart);
        }

        internal void CommandBatchExecute(string parameterPart)
        {
            parameterPart = IrtKernel.RemoveParenthesis(parameterPart, IrtConst.BaseOpen, IrtConst.BaseClose);
            parameterPart = IrtHelper.ReadBatchFile(parameterPart);

            if (string.IsNullOrEmpty(parameterPart))
            {
                _irtHandleInternal.SetError(IrtConst.ErrorFileNotFound);
                return;
            }

            GenerateCommands(parameterPart);
        }

        private void GenerateCommands(string parameterPart)
        {
            var commands = IrtKernel.SplitParameter(parameterPart, IrtConst.NewCommand).ToList();
            var currentPosition = 0;

            while (currentPosition < commands.Count)
            {
                var com = commands[currentPosition];
                var key = IrtKernel.CheckForKeyWord(com, IrtConst.InternContainerCommands);

                if (key == IrtConst.Error)
                {
                    // Unknown command, treat as normal command
                    _prompt.AddToLog(com);
                    _prompt.ConsoleInput(com);
                    _parsedCommands.Add("COMMAND", currentPosition, com);
                }
                else
                {
                    switch (key)
                    {
                        case 0: // if
                            _ifDepth++;
                            _parsedCommands.Add($"IF_{_ifDepth}", currentPosition, com);
                            // TODO: Implement actual if block processing if needed
                            // e.g. await HandleIfElseBlock(commands, currentPosition);
                            break;
                        case 1: // else
                            _parsedCommands.Add($"ELSE_{_ifDepth}", currentPosition, com);
                            break;
                        case 2: // goto
                            _parsedCommands.Add("GOTO", currentPosition, com);
                            currentPosition = IsJumpCommand(com, key, out var jumpPosition, commands)
                                ? Math.Clamp(jumpPosition, 0, commands.Count - 1)
                                : IrtConst.Error;
                            break;
                        case 3: // label
                            _parsedCommands.Add("LABEL", currentPosition, com);
                            break;
                    }
                }

                if (currentPosition == IrtConst.Error)
                {
                    var message = Logging.SetLastError($"{IrtConst.JumpLabelNotFoundError}{com}", 0);
                    _prompt.SendLogs(this, message);
                    break;
                }

                currentPosition++;
            }

            // At this point, _parsedCommands holds the full categorized command list
            // You can expose it or process further as needed
            Trace.WriteLine($"Parsed {_parsedCommands.Count} commands.");
        }

        // Placeholder for actual if/else block handling (expand as needed)
        private async Task<int> HandleIfElseBlock(List<string> commands, int currentPosition)
        {
            // TODO: Implement evaluation of the if condition and command execution
            // You may want to parse nested blocks, execute or skip based on condition

            // Example placeholder:
            // bool conditionMet = EvaluateCondition(commands[currentPosition]);
            // if (conditionMet) { ExecuteIfBlock(); } else { ExecuteElseBlock(); }

            return currentPosition;
        }

        private static bool IsJumpCommand(string input, int key, out int position, IReadOnlyList<string> commands)
        {
            position = 0;

            var (status, label) = IrtKernel.GetParameters(input, key, IrtConst.InternContainerCommands);

            if (status != IrtConst.ParameterCommand || string.IsNullOrEmpty(label))
            {
                return false;
            }

            position = FindLabelPosition(label, commands);

            return position >= 0;
        }

        private static int FindLabelPosition(string label, IReadOnlyList<string> commands)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var input = commands[i];
                var check = IrtKernel.CheckFormat(input, IrtConst.InternalLabel, label);
                if (check)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Optional helper to parse a single line into categories if needed externally.
        /// </summary>
        /// <param name="line">The command line</param>
        /// <param name="lineCount">Line index</param>
        /// <param name="ifDepth">Current if depth</param>
        private void ParseLine(string line, int lineCount, int ifDepth)
        {
            if (line.StartsWith(":")) // Label
            {
                _parsedCommands.Add("LABEL", lineCount, line);
            }
            else if (line.Trim().StartsWith("goto")) // Goto
            {
                _parsedCommands.Add("GOTO", lineCount, line);
            }
            else if (line.Trim().StartsWith("if")) // If
            {
                _parsedCommands.Add($"IF_{ifDepth + 1}", lineCount, line);
            }
            else if (line.Trim().StartsWith("else")) // Else
            {
                _parsedCommands.Add($"ELSE_{ifDepth}", lineCount, line);
            }
            else // Default
            {
                _parsedCommands.Add("COMMAND", lineCount, line);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _irtHandleInternal = null;
            }

            _prompt = null;

            _disposed = true;
        }

        ~IrtHandleScript()
        {
            Dispose(false);
        }
    }
}
