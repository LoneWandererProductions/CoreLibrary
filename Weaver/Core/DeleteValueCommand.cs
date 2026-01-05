/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        DeleteValueCommand.cs
 * PURPOSE:     Deletes a stored value from the Scripter Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Mostly internal command, deletes a value from the Scripter registry.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class DeleteValueCommand : ICommand
    {
        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteValueCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public DeleteValueCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "deleteValue";

        /// <inheritdoc />
        public string Description => "Deletes a variable from the registry.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length != 1)
                return CommandResult.Fail("Usage: deleteValue(key)");

            var key = args[0];
            return _registry.Remove(key)
                ? CommandResult.Ok($"Deleted variable '{key}'.")
                : CommandResult.Fail($"Variable '{key}' not found.");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail("'deleteValue' has no extensions.");
    }
}