/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        HelpCommand.cs
 * PURPOSE:     Basic internal help command. Works also for external commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Commands
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, shows basic information about available commands and about Weaver itself.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class HelpCommand : ICommand
    {
        /// <summary>
        /// The get commands
        /// </summary>
        private readonly Func<IEnumerable<ICommand>> _getCommands;

        /// <summary>
        /// The get extensions
        /// </summary>
        private readonly Func<List<ICommandExtension>> _getExtensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommand" /> class.
        /// </summary>
        /// <param name="getCommands">The get commands.</param>
        /// <param name="getExtensions">The get extensions.</param>
        public HelpCommand(Func<IEnumerable<ICommand>> getCommands, Func<List<ICommandExtension>> getExtensions)
        {
            _getCommands = getCommands;
            _getExtensions = getExtensions;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => WeaverResources.GlobalCommandHelp;

        /// <inheritdoc />
        public string Description =>
            "Lists all commands or shows information about a specific command. Usage: help([commandName]).";

        /// <inheritdoc />
        public int ParameterCount => 0; // we’ll allow 0 or 1 dynamically

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            // 1️⃣ No arguments → List everything grouped by Namespace
            if (args.Length == 0)
            {
                var allCommands = _getCommands();
                var allExtensions = _getExtensions();

                // Merge both into a single anonymous structure for unified grouping
                var combined = allCommands
                    .Select(c => new { c.Name, c.Description, c.Namespace, Prefix = "" })
                    .Concat(allExtensions.Select(e => new { e.Name, e.Description, e.Namespace, Prefix = "." }));

                var grouped = combined
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Namespace) ? "Global" : x.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var entries = string.Join("\n", g.Select(x => $"  {x.Prefix}{x.Name} — {x.Description}"));
                        return $"[{g.Key.ToUpperInvariant()}]:\n{entries}";
                    });

                var text = string.Join("\n\n", grouped);

                return CommandResult.Ok(
                    "Weaver Cmd version 0.5 — made by Peter Geinitz (Wayfarer).\n\n" + text
                );
            }

            // 2️⃣ One argument → Look up Command OR Extension description
            if (args.Length == 1)
            {
                var targetName = args[0].TrimStart('.'); // Handle "help(.store)" or "help(store)"

                // Search Commands first
                var cmdMatch = _getCommands().FirstOrDefault(c =>
                    c.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                if (cmdMatch != null)
                    return CommandResult.Ok($"{cmdMatch.Namespace}:{cmdMatch.Name} — {cmdMatch.Description}");

                // Search Extensions second
                var extMatch = _getExtensions().FirstOrDefault(e =>
                    e.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                if (extMatch != null)
                {
                    var ns = string.IsNullOrEmpty(extMatch.Namespace) ? "Global" : extMatch.Namespace;
                    return CommandResult.Ok($".{extMatch.Name} (Extension) [{ns}] — {extMatch.Description}");
                }

                return CommandResult.Fail($"Unknown command or extension '{targetName}'.");
            }

            // 3️⃣ Error handling
            return CommandResult.Fail("Usage: help() to list all, or help(name) for details.");
        }
    }
}