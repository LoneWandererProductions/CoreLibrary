/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Lector
 * FILE:        SampleExtension.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

#nullable enable
using Weaver.Interfaces;
using Weaver.Messages;

namespace Lector
{
    /// <inheritdoc />
    /// <summary>
    /// Sample Extension.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommandExtension" />
    public sealed class SampleExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "sample"; // name of the extension

        /// <inheritdoc />
        public string Description => "A sample extension that wraps command execution.";

        /// <inheritdoc />
        public string Namespace => "system";

        /// <inheritdoc />
        /// <summary>
        /// Wraps the execution of the command. Can run before/after logic or inject feedback.
        /// </summary>
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            // Before hook
            BeforeExecute(command, args);

            // Optionally, preview or manipulate the execution
            var result = executor(args);

            // After hook
            AfterExecute(command, result);

            // Optionally inject feedback or modify result
            if (command.Name.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                if (!result.RequiresConfirmation)
                {
                    result = new CommandResult
                    {
                        Message = $"[SampleExtension] Executed {command.Name}.",
                        RequiresConfirmation = false,
                        Success = result.Success
                    };
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void BeforeExecute(ICommand command, string[]? args = null)
        {
            // Optional pre-processing
            Console.WriteLine($"[SampleExtension] Before executing {command.Name}");
        }

        /// <inheritdoc />
        public void AfterExecute(ICommand command, CommandResult result)
        {
            // Optional post-processing
            Console.WriteLine($"[SampleExtension] After executing {command.Name}: Success={result.Success}");
        }
    }
}
