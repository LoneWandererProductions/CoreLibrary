/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        MemClear.cs
 * PURPOSE:     Internal Command,for scripting Engine, clear variables.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Commands
{
    /// <summary>
    /// Memory management command, clears all stored variables or a specific variable.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class MemClearCommand : ICommand
    {
        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemClearCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public MemClearCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "memClear";

        /// <inheritdoc />
        public string Description =>
            "Clears all memory or only a defined variable.  Usage: MemClear([variable: optional])";

        /// <inheritdoc />
        public int ParameterCount => 0;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (_registry == null)
                return CommandResult.Fail("Variable registry is not available.");

            if (args.Length > 1)
                return CommandResult.Fail("Usage: MemClear([variable]) or MemClear().");

            if (args.Length == 0)
            {
                _registry.ClearAll();
                return CommandResult.Ok("Memory was cleared.");
            }

            var variable = args[0];

            return _registry.Remove(variable)
                ? CommandResult.Ok($"{variable} was cleared.")
                : CommandResult.Fail($"{variable} does not exist.");
        }
    }
}