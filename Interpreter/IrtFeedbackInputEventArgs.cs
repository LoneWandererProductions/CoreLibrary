/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtFeedbackInputEventArgs.cs
 * PURPOSE:     Prepare the feedback of the user input to the fitting Parser
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Interpreter
{
    /// <summary>
    ///     EventArgs that gets delievered to all Listener, the right listener gets identified by: RequestId
    ///     This strange construct is needed for batch commands
    /// </summary>
    internal sealed class IrtFeedbackInputEventArgs
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
    }
}
