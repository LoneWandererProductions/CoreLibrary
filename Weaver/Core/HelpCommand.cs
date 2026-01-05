/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        HelpCommand.cs
 * PURPOSE:     Basic internal help command. Works also for external commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, shows basic information about available commands and about Weaver itself.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class HelpCommand : ICommand
    {
        /// <summary>
        /// The get commands
        /// </summary>
        private readonly Func<IEnumerable<ICommand>> _getCommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommand"/> class.
        /// </summary>
        /// <param name="getCommands">The get commands.</param>
        public HelpCommand(Func<IEnumerable<ICommand>> getCommands)
        {
            _getCommands = getCommands;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => WeaverResources.GlobalCommandHelp;

        /// <inheritdoc />
        public string Description => "Lists all commands or shows information about a specific command.";

        /// <inheritdoc />
        public int ParameterCount => 0; // we’ll allow 0 or 1 dynamically

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            // 1️⃣ No arguments → simple static help
            if (args.Length == 0)
            {
                return CommandResult.Ok("Weaver Cmd version 0.5 — made by Peter Geinitz (Wayfarer).");
            }

            // 2️⃣ One argument → look up command description
            if (args.Length == 1)
            {
                var cmdName = args[0];
                var match = _getCommands().FirstOrDefault(c =>
                    c.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                    return CommandResult.Ok($"{match.Namespace}:{match.Name} — {match.Description}");

                return CommandResult.Fail($"Unknown command '{cmdName}'.");
            }

            // 3️⃣ More than one argument → optional, you could return syntax hint
            return CommandResult.Fail("Usage: help [commandName]");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"'{Name}' has no extensions.");
        }
    }
}