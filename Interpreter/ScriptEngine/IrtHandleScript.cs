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
using ExtendedSystemObjects;
using Interpreter.Resources;

namespace Interpreter.ScriptEngine;

/// <inheritdoc />
/// <summary>
///     Handle our Container
/// </summary>
/// <seealso cref="IDisposable" />
public sealed class IrtHandleScript : IDisposable
{
    // Persistent storage for parsed and categorized commands
    private readonly CategorizedDictionary<int, string> _parsedCommands = new();
    private bool _disposed;

    // Track nesting level of if blocks
    private int _ifDepth;
    private IrtHandleInternal? _irtHandleInternal;
    private Prompt? _prompt;

    /// <summary>
    /// Prevents a default instance of the <see cref="IrtHandleScript"/> class from being created.
    /// </summary>
    private IrtHandleScript()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IrtHandleScript"/> class.
    /// </summary>
    /// <param name="irtHandleInternal">The irt handle internal.</param>
    /// <param name="prompt">The prompt.</param>
    internal IrtHandleScript(IrtHandleInternal irtHandleInternal, Prompt prompt)
    {
        _irtHandleInternal = irtHandleInternal;
        _prompt = prompt;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Commands the container.
    /// </summary>
    /// <param name="parameterPart">The parameter part.</param>
    internal void CommandContainer(string parameterPart)
    {
        parameterPart = IrtKernel.CutLastOccurrence(parameterPart, IrtConst.AdvancedClose);
        parameterPart = IrtKernel.RemoveFirstOccurrence(parameterPart, IrtConst.AdvancedOpen);

        GenerateCommands(parameterPart);
    }

    /// <summary>
    /// Commands the batch execute.
    /// </summary>
    /// <param name="parameterPart">The parameter part.</param>
    internal void CommandBatchExecute(string parameterPart)
    {
        parameterPart = IrtKernel.RemoveParenthesis(parameterPart, IrtConst.BaseOpen, IrtConst.BaseClose);
        parameterPart = IrtHelper.ReadBatchFile(parameterPart);

        if (string.IsNullOrEmpty(parameterPart))
        {
            _irtHandleInternal?.SetError(IrtConst.ErrorFileNotFound);
            return;
        }

        GenerateCommands(parameterPart);
    }

    /// <summary>
    /// Generates the commands.
    /// </summary>
    /// <param name="parameterPart">The parameter part.</param>
    private void GenerateCommands(string parameterPart)
    {
        var commands = IrtKernel.SplitParameter(parameterPart, IrtConst.NewCommand).ToList();

        for (var currentPosition = 0; currentPosition < commands.Count; currentPosition++)
        {
            var com = commands[currentPosition];
            var key = IrtKernel.CheckForKeyWord(com, IrtConst.InternContainerCommands);

            if (key == IrtConst.Error)
            {
                // Unknown command, treat as normal command
                _prompt?.AddToLog(com);
                _prompt?.ConsoleInput(com);
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
                _prompt?.SendLogs(this, message);
                break;
            }
        }

        // At this point, _parsedCommands holds the full categorized command list
        // You can expose it or process further as needed
        Trace.WriteLine($"Parsed {_parsedCommands.Count} commands.");
    }


    /// <summary>
    /// Determines whether [is jump command] [the specified input].
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="key">The key.</param>
    /// <param name="position">The position.</param>
    /// <param name="commands">The commands.</param>
    /// <returns>
    ///   <c>true</c> if [is jump command] [the specified input]; otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Finds the label position.
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="commands">The commands.</param>
    /// <returns>Label Position.</returns>
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
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

    /// <summary>
    /// Finalizes an instance of the <see cref="IrtHandleScript"/> class.
    /// </summary>
    ~IrtHandleScript()
    {
        Dispose(false);
    }
}
