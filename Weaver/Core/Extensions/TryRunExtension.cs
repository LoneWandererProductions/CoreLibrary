/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Extensions
 * FILE:        TryRunExtension.cs
 * PURPOSE:     Provides a preview of command execution with optional confirmation.
 *              This extension takes full control of the execution flow and may
 *              invoke the underlying command if the user confirms.
 *              For example, HelpExtension does not need a TryRun; it can live without it.
 *              Historically ported into the system as it’s useful for many commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Extensions
{
    /// <summary>
    /// Provides a preview of command execution and requests user confirmation.
    /// Wraps the command execution in a feedback request.
    /// </summary>
    public sealed class TryRunExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => WeaverResources.GlobalExtensionTryRun;

        /// <inheritdoc />
        public string Description =>
            "Provides a preview of command execution and requests user confirmation.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        /// <remarks>
        /// Extension workflow:
        /// 1. Calls TryRun() on the command (if implemented, otherwise null).
        /// 2. Wraps the preview in a FeedbackRequest.
        /// 3. Executes the command via the provided executor if confirmed.
        /// 4. Handles invalid input by prompting the user again.
        /// </remarks>
        public CommandResult Invoke(
            ICommand command,
            string[] extensionArgs,
            Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            CommandResult preview;

            // 1️⃣ If command has a TryRun() method, call it
            var tryRunMethod = command.GetType()
                .GetMethod("TryRun", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (tryRunMethod != null && tryRunMethod.ReturnType == typeof(CommandResult))
            {
                preview = (CommandResult)tryRunMethod.Invoke(command, new object?[] { commandArgs })!;
            }
            // 2️⃣ Else fallback: run executor but mark as preview
            else
            {
                var fallback = executor(commandArgs);
                preview = new CommandResult
                {
                    Message = $"[Preview-Fallback] {fallback.Message}",
                    Success = fallback.Success
                };
            }

            // 3️⃣ Feedback wrapper
            FeedbackRequest? feedback = null;
            var cache = feedback;

            feedback = new FeedbackRequest(
                prompt: $"Preview:\n{preview.Message}\nProceed with execution? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => executor(commandArgs),
                        "no" => CommandResult.Fail("Execution cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no",
                            RequiresConfirmation = true,
                            Feedback = cache
                        }
                    };
                });

            return new CommandResult
            {
                Message = $"{preview.Message}\n{feedback.Prompt}",
                Feedback = feedback,
                RequiresConfirmation = true,
                Success = false
            };
        }
    }
}