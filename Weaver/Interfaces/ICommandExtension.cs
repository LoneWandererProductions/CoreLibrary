/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        ICommandExtension.cs
 * PURPOSE:     Interface for external command extensions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.Interfaces
{
    /// <summary>
    /// Represents an external command extension (e.g. .help, .tryrun)
    /// that can be plugged into the Weaver runtime to extend command behavior.
    /// </summary>
    public interface ICommandExtension
    {
        /// <summary>
        /// The extension name (e.g. "help", "tryrun").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }

        /// <summary>
        /// Optional namespace, for scoping (e.g. "system", "user", etc.)
        /// If null or empty, applies globally.
        /// </summary>
        string? Namespace { get; }

        /// <summary>
        /// Gets the extension parameter count.
        /// </summary>
        /// <value>
        /// The extension parameter count.
        /// </value>
        int ExtensionParameterCount => 0;

        /// <summary>
        /// Executes the extension logic for the given command.
        /// </summary>
        /// <param name="command">The command that this extension applies to.</param>
        /// <param name="extensionArgs">The extension arguments.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="commandArgs">The command arguments.</param>
        /// <returns>
        /// A <see cref="CommandResult" /> representing the outcome of the extension execution.
        /// </returns>
        CommandResult Invoke(
            ICommand command,
            string[] extensionArgs,  // now clearly only for the extension
            Func<string[], CommandResult> executor,
            string[] commandArgs     // original command args
        );

        /// <summary>
        /// Optional: can run pre/post hooks around command execution.
        /// Before the execution.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="args">The arguments.</param>
        void BeforeExecute(ICommand command, string[]? args = null)
        {
        }

        /// <summary>
        ///  Optional: can run pre/post hooks around command execution.
        /// After the execution.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="result">The result.</param>
        void AfterExecute(ICommand command, CommandResult result)
        {
        }
    }
}