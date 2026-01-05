/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        PrintCommand.cs
 * PURPOSE:     Basic message Print command. Moslty used for script Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, prints a message.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class PrintCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrintCommand"/> class.
        /// </summary>
        public PrintCommand()
        {
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "Print";

        /// <inheritdoc />
        public string Description => "Print the input message on screen";

        /// <inheritdoc />
        public int ParameterCount => 1; // we’ll allow 0 or 1 dynamically

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            // 1️⃣ No arguments → fail
            if (args.Length == 0)
            {
                return CommandResult.Fail("No Input provided.");
            }

            // 2️⃣ One argument → print it
            if (args.Length == 1)
            {
                var message = args[0];

                if (!string.IsNullOrEmpty(message))
                    return CommandResult.Ok(message);

                //just return fail if empty
                return CommandResult.Fail("");
            }

            // 3️⃣ More than one argument → optional, you could return syntax hint
            return CommandResult.Fail("Usage: print([message])");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"'{Name}' has no extensions.");
        }
    }
}