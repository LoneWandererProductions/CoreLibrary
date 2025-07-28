/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/InCommand.cs
 * PURPOSE:     The In Command, a list of commands provided external
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Diagnostics;
using Interpreter.Resources;

namespace Interpreter;

/// <summary>
///     Simple Element of the Register
/// </summary>
[DebuggerDisplay("{DebugDisplay,nq}")]
public sealed class InCommand
{
    /// <summary>
    ///     Gets the debug display.
    /// </summary>
    /// <value>
    ///     The debug display.
    /// </value>
    private string DebugDisplay =>
        $"{Command ?? "<null>"} | {Description ?? "<null>"} | {ExpectedReturnType?.Name ?? "void"}";

    /// <summary>
    ///     Sets the optional feedback identifier to User Feedback.
    /// </summary>
    /// <value>
    ///     The feedback identifier.
    /// </value>
    public int FeedbackId { internal get; init; }

    /// <summary>
    ///     Gets or sets the command.
    /// </summary>
    public string Command { internal get; init; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    public string Description { internal get; init; }

    /// <summary>
    ///     Gets or sets the Parameter count.
    ///     If positive Amount = Amount
    ///     If negative Amount >= Absolute(amount)
    /// </summary>
    public int ParameterCount { internal get; init; }

    /// <summary>
    ///     If this command returns a result, define the expected type (optional).
    ///     Null means no return value (void).
    /// </summary>
    public Type ExpectedReturnType { get; set; }

    /// <summary>
    ///     Convenience helper for readability.
    /// </summary>
    public bool HasReturn => ExpectedReturnType != null;

    /// <summary>
    ///     Converts to string.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return string.Concat(
            Command ?? "<null>",
            IrtConst.Separator,
            Description ?? "<null>",
            IrtConst.Separator,
            ExpectedReturnType?.ToString() ?? "void"
        );
    }

    /// <summary>
    ///     Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Command, ParameterCount);
    }
}
