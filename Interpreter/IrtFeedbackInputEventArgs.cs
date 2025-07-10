/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtFeedbackInputEventArgs.cs
 * PURPOSE:     Prepare the feedback of the user input to the fitting Parser
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using Interpreter.Resources;

namespace Interpreter
{
    /// <summary>
    ///     EventArgs that gets delievered to all Listener, the right listener gets identified by: RequestId
    ///     This strange construct is needed for batch commands
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class IrtFeedbackInputEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets or sets the input.
        /// </summary>
        /// <value>
        ///     The input.
        /// </value>
        internal string Input { get; set; }

        /// <summary>
        ///     Gets or sets the request identifier.
        /// </summary>
        /// <value>
        ///     The request identifier.
        /// </value>
        internal string RequestId { get; init; }

        /// <summary>
        ///     Gets or sets the branch identifier.
        /// </summary>
        /// <value>
        ///     The branch identifier.
        /// </value>
        internal int BranchId { get; init; }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        internal int Key { get; init; }

        /// <summary>
        ///     Gets or sets the command.
        /// </summary>
        /// <value>
        ///     The command.
        /// </value>
        internal string Command { get; init; }

        /// <summary>
        ///     Gets or sets the awaited output.
        /// </summary>
        /// <value>
        ///     The awaited output.
        /// </value>
        internal OutCommand AwaitedOutput { get; init; }

        /// <summary>
        ///     Gets or sets the answer.
        /// </summary>
        /// <value>
        ///     The answer.
        /// </value>
        internal AvailableFeedback Answer { get; init; }

        /// <summary>
        ///     Provides a debugger-friendly display string.
        /// </summary>
        private string DebuggerDisplay => ToString();

        /// <summary>
        ///     Returns a string that represents the current feedback input event.
        /// </summary>
        /// <returns>A string representation with key properties for quick inspection.</returns>
        public override string ToString()
        {
            return
                $"RequestId: {RequestId ?? "null"}, " +
                $"BranchId: {BranchId}, " +
                $"Key: {Key}, " +
                $"Command: {Command ?? "null"}, " +
                $"Answer: {Answer}, " +
                $"AwaitedOutput: {AwaitedOutput?.ToString() ?? "null"}, " +
                $"Input: \"{(string.IsNullOrEmpty(Input) ? "<empty>" : Input)}\"";
        }
    }
}
