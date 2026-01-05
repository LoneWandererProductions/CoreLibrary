/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        HelpExtension.cs
 * PURPOSE:     Global extension to provide help information for a command, directly.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Provides help information for a command using the .help extension.
    /// </summary>
    public sealed class HelpExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => WeaverResources.GlobalExtensionHelp;

        /// <inheritdoc />
        public string Description => "Provides help information for a command using the .help extension.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ExtensionParameterCount => 0;

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] extensionArgs, Func<string[], CommandResult> executor, string[] commandArgs)
        {
            // Return description; executor not needed
            return CommandResult.Ok(command.Description);
        }
    }
}