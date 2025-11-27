/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        MemoryCommand.cs
 * PURPOSE:     Internal Command, for scripting Engine, lists all stored variables.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Internal Command, for scripting Engine, lists all stored variables.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class MemoryCommand : ICommand
    {
        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public MemoryCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "memory";

        /// <inheritdoc />
        public string Description => "Lists all stored variables with values and types.";

        /// <inheritdoc />
        public int ParameterCount => 0;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var content = _registry.ToString();
            return CommandResult.Ok(content);
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"'{Name}' has no extensions.");
        }
    }
}