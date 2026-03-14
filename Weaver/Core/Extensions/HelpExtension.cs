/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Extensions
 * FILE:        HelpExtension.cs
 * PURPOSE:     Global extension to provide help information for a command, directly.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Extensions
{
    /// <summary>
    /// Provides help information for a command using the .help extension.
    /// </summary>
    public sealed class HelpExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => WeaverResources.GlobalExtensionHelp;

        /// <inheritdoc />
        public string Description =>
            "Provides help information for a command using the .help extension, Example: command.help().";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ExtensionParameterCount => 0;

        /// <inheritdoc />
        public CommandResult Invoke(
            ICommand command,
            string[] extensionArgs,
            Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            var sb = new StringBuilder();

            // 1️⃣ Command basic info
            sb.AppendLine($"Command: {command.Name}");
            sb.AppendLine($"Namespace: {command.Namespace}");
            sb.AppendLine($"Description: {command.Description}");
            sb.AppendLine($"Parameter count: {command.ParameterCount}");
            sb.AppendLine();

            // 2️⃣ List available extensions (if any)
            if (command.Extensions is { Count: > 0 })
            {
                sb.AppendLine("Available extensions:");
                foreach (var kvp in command.Extensions)
                {
                    var paramInfo = kvp.Value < 0 ? "variable parameters" : $"{kvp.Value} parameter(s)";
                    sb.AppendLine($" - {kvp.Key} ({paramInfo})");
                }
            }
            else
            {
                sb.AppendLine("No extensions available for this command.");
            }

            return CommandResult.Ok(sb.ToString());
        }
    }
}