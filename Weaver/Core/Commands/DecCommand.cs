/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        IncDecCommands.cs
 * PURPOSE:     Increment or decrement a numeric value in the Scripter registry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Commands
{
    /// <summary>
    /// Decrements a numeric value in the registry.
    /// </summary>
    public sealed class DecCommand : ICommand
    {
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public DecCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "Dec";

        /// <inheritdoc />
        public string Description => "Decrements a numeric variable by 1. Usage: Dec([key])";

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
                return CommandResult.Fail("Usage: Dec([key])");

            var key = args[0];
            if (string.IsNullOrWhiteSpace(key))
                return CommandResult.Fail("Key cannot be empty.");

            if (!_registry.TryGet(key, out var value, out var type))
                return CommandResult.Fail($"Key '{key}' not found.");

            try
            {
                switch (type)
                {
                    case EnumTypes.Wint:
                        value = Convert.ToInt32(value) - 1;
                        break;
                    case EnumTypes.Wdouble:
                        value = Convert.ToDouble(value) - 1.0;
                        break;
                    default:
                        return CommandResult.Fail($"Dec only supports numeric types. Current type: {type}");
                }

                _registry.Set(key, value, type);
                return CommandResult.Ok($"Decremented '{key}' to {value}.", value, type);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to decrement '{key}': {ex.Message}");
            }
        }
    }
}
