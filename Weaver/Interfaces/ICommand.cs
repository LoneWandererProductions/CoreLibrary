/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        ICommand.cs
 * PURPOSE:     Declaration of the command interface for Weaver commands. Defines properties and methods that all commands must implement.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.Interfaces
{
    /// <summary>
    /// Interface and backbone for all Weaver commands.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        string Description { get; }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        string Namespace { get; }

        /// <summary>
        /// Gets the signature.
        /// </summary>
        /// <value>
        /// The signature.
        /// </value>
        CommandSignature Signature { get; }

        /// <summary>
        /// Gets the parameter count.
        /// </summary>
        /// <value>
        /// The parameter count.
        /// </value>
        int ParameterCount => 0; // default means variable

        /// <summary>
        /// Executes the command with given arguments.
        /// Returns a result that can include text, status, or further options.
        /// </summary>
        CommandResult Execute(params string[] args);

        /// <summary>
        /// Optional extension calls like .help(), .save(), .tryrun()
        /// </summary>
        CommandResult InvokeExtension(string extensionName, params string[] args);

        /// <summary>
        /// Optional: expose available extensions with parameter counts
        /// </summary>
        IReadOnlyDictionary<string, int>? Extensions => null;

        /// <summary>
        /// Optional preview mode: called by .tryrun().
        /// Returns a preview result without committing side effects.
        /// </summary>
        CommandResult? TryRun(params string[] args)
        {
            return InvokeExtension("tryrun", args);
        }
    }
}