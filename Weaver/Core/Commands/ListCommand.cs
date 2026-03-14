/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        ListCommand.cs
 * PURPOSE:     Basic message Print command.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Commands
{
    /// <inheritdoc />
    /// <summary>
    /// Internal command, Prints a message.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class ListCommand : ICommand
    {
        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "list";

        /// <inheritdoc />
        public string Description => "Lists all commands.";

        /// <inheritdoc />
        public int ParameterCount => 0; // we’ll allow 0

        /// <summary>
        /// The get commands method.
        /// </summary>
        private readonly Func<IEnumerable<ICommand>> _getCommands;

        /// <summary>
        /// The get extensions  method.
        /// </summary>
        private readonly Func<IEnumerable<ICommandExtension>> _getExtensions;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand" /> class.
        /// </summary>
        /// <param name="getCommands">The get commands.</param>
        /// <param name="getExtensions">The get extensions.</param>
        public ListCommand(Func<IEnumerable<ICommand>> getCommands, Func<List<ICommandExtension>> getExtensions)
        {
            _getCommands = getCommands;
            _getExtensions = getExtensions;
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var allCommands = _getCommands();
            var allExtensions = _getExtensions();

            // 1. Project both into a common format (Name, Description, Namespace) 
            // to group them together under the same headers
            var combined = allCommands
                .Select(c => new { c.Name, c.Description, c.Namespace, Type = "Command" })
                .Concat(allExtensions.Select(e => new { e.Name, e.Description, e.Namespace, Type = "Extension" }));

            // 2. Group by Namespace
            var grouped = combined
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Namespace) ? "Global" : x.Namespace)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var entries = string.Join("\n", g.Select(x =>
                        x.Type == "Extension"
                            ? $"  .{x.Name} (ext) — {x.Description}"
                            : $"  {x.Name} — {x.Description}"));

                    return $"[{g.Key.ToUpperInvariant()}]\n{entries}";
                });

            var finalOutput = "Available Registry:\n\n" + string.Join("\n\n", grouped);

            return CommandResult.Ok(finalOutput);
        }
    }
}