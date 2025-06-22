/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtFeedback.cs
 * PURPOSE:     Handle some stuff from the User Feedback
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using Interpreter.Resources;

namespace Interpreter
{
    /// <summary>
    ///     Control Object that checks if User Feedback is needed
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    internal sealed class IrtFeedback
    {
        /// <summary>
        ///     Gets or sets the key of the command.
        /// </summary>
        /// <value>
        ///     The key.
        /// </value>
        internal int Key { get; init; }

        /// <summary>
        ///     Gets the request identifier.
        /// </summary>
        /// <value>
        ///     The request identifier.
        /// </value>
        internal string RequestId { get; init; }

        /// <summary>
        ///     Gets the feedback.
        /// </summary>
        /// <value>
        ///     The feedback.
        /// </value>
        internal UserFeedback Feedback { get; init; }

        /// <summary>
        ///     Gets the branch identifier.
        /// </summary>
        /// <value>
        ///     The branch identifier.
        /// </value>
        internal int BranchId { get; init; }

        /// <summary>
        ///     Gets the command.
        /// </summary>
        /// <value>
        ///     The command.
        /// </value>
        internal string Command { get; init; }

        /// <summary>
        ///     Gets the awaited output.
        /// </summary>
        /// <value>
        ///     The awaited output.
        /// </value>
        internal OutCommand AwaitedOutput { get; init; }

        /// <summary>
        ///     Generates the feedback answer.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <returns>Answer Event</returns>
        internal IrtFeedbackInputEventArgs GenerateFeedbackAnswer(AvailableFeedback answer)
        {
            return new IrtFeedbackInputEventArgs
            {
                Command = Command,
                Key = Key,
                RequestId = RequestId,
                BranchId = BranchId,
                AwaitedOutput = AwaitedOutput,
                Answer = answer
            };
        }

        /// <summary>
        ///     Returns a string representation of this feedback.
        /// </summary>
        public override string ToString()
        {
            return
                $"[Feedback: Cmd={Command}, Key={Key}, ReqId={RequestId}, Branch={BranchId}, Output={AwaitedOutput}]";
        }
    }
}
