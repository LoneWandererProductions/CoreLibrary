/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Extensions
 * FILE:        CleanExtension.cs
 * PURPOSE:     Global extension to provide a simple way to wipe the registry entry associated with a command, effectively clearing any stored data or state. Example: command.clean().
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Extensions
{
    /// <summary>
    /// This is a global extension that can be used with any command that implements IRegistryProducer. It wipes the registry entry associated with the command, effectively clearing any stored data or state.
    /// This can be useful for resetting a command's state or ensuring that sensitive information is removed from memory after use.
    /// </summary>
    /// <seealso cref="ICommandExtension" />
    public sealed class CleanExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => WeaverResources.GlobalExtensionClean;

        /// <inheritdoc />
        public string Description => "Wipes the last used registry entry associated with a command.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] extensionArgs, Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            // 1. Check if the command is a producer
            if (command is not IRegistryProducer producer)
            {
                return CommandResult.Fail(
                    $"Command '{command.Name}' does not support registry operations (IRegistryProducer not implemented).");
            }

            try
            {
                // 2. Identify the target
                var registry = producer.Variables;
                var key = producer.CurrentRegistryKey;

                // 3. Perform the wipe
                // Depending on your IVariableRegistry, this might be a Remove or a Null set
                if (registry.TryGetObject(key, out _))
                {
                    registry.Remove(key);
                    return CommandResult.Ok($"Successfully cleaned registry key '${key}'.", key, producer.DataType);
                }

                return CommandResult.Ok($"Registry key '${key}' was already empty.", key, producer.DataType);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Clean extension failed: {ex.Message}");
            }
        }
    }
}
