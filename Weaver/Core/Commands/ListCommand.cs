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

        private readonly Func<IEnumerable<ICommand>> _getCommands;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCommand"/> class.
        /// </summary>
        /// <param name="getCommands">The get commands.</param>
        public ListCommand(Func<IEnumerable<ICommand>> getCommands)
        {
            _getCommands = getCommands;
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            var allCommands = _getCommands();

            var grouped = allCommands
                .GroupBy(c => c.Namespace)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var entries = string.Join("\n", g.Select(c => $"  {c.Name} — {c.Description}"));
                    return $"{g.Key}:\n{entries}";
                });

            return CommandResult.Ok("Available commands:\n\n" + string.Join("\n\n", grouped));
        }
    }
}