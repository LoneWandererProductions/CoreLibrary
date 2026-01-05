/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        StoreExtension.cs
 * PURPOSE:     Global extension to store results of a command into the variable registry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Provides help information for a command using the .help extension.
    /// </summary>
    public sealed class StoreExtension : ICommandExtension
    {
        /// <summary>
        /// The registry
        /// </summary>
        private IVariableRegistry _registry;

        /// <inheritdoc />
        public string Name => WeaverResources.GlobalExtensionStore;

        /// <inheritdoc />
        public string Description => "Stores result of a command using the .store extension. Usual store is 'result', you can provide your own key via overload.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ExtensionParameterCount => -1;

        public StoreExtension(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] extensionArgs, Func<string[], CommandResult> executor, string[] commandArgs)
        {
            // Determine key from extension args
            var key = extensionArgs.Length > 0 && !string.IsNullOrWhiteSpace(extensionArgs[0])
                ? extensionArgs[0]
                : "result";

            // Execute command with its proper arguments
            var result = executor(commandArgs);

            if (!result.Success)
                return result;

            // Store result
            _registry.Set(key, result.Value, result.Type);

            return CommandResult.Ok($"Stored result as '{key}'.", result.Value);
        }
    }
}