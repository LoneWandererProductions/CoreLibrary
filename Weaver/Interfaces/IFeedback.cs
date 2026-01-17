/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        ICommandExtension.cs
 * PURPOSE:     Interface for feedback handling in commands and extensions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.Interfaces
{
    /// <summary>
    /// Encapsulates a feedback request and response flow for commands or extensions.
    /// </summary>
    public interface IFeedback
    {
        /// <summary>
        /// The prompt shown to the user.
        /// </summary>
        /// <value>
        /// The prompt.
        /// </value>
        string Prompt { get; }

        /// <summary>
        /// Possible valid options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        IReadOnlyList<string> Options { get; }

        /// <summary>
        /// Indicates if feedback is still pending.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        bool IsPending { get; }

        /// <summary>
        /// Handles a user response and returns a CommandResult.
        /// </summary>
        /// <param name="input">User input</param>
        /// <returns>The Command result object.</returns>
        CommandResult Respond(string input);
    }
}