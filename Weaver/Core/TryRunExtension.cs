/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        TryRunExtension.cs
 * PURPOSE:     Provides a preview of command execution with optional confirmation
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <summary>
    /// Provides a preview of command execution and requests user confirmation.
    /// The extension takes full control of the execution flow and may decide
    /// whether or not to invoke the underlying command via the provided executor.
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
        /// The extension:
        /// 1. Executes a preview (via TryRun if available, otherwise via executor).
        /// 2. Creates a feedback request to ask the user whether to proceed.
        /// 3. Uses the provided executor to run the command if confirmed.
        /// 4. Handles invalid feedback input by prompting the user again.
        /// </remarks>
        public CommandResult Invoke(
             ICommand command,
             string[] extensionArgs,
             Func<string[], CommandResult> executor,
             string[] commandArgs)
        {
            // Step 1: Preview execution using commandArgs
            var preview = command.TryRun(commandArgs) ?? executor(commandArgs);

            // Step 2: Declare feedback reference (needed for recursive retry)
            FeedbackRequest? feedback = null;

            // Step 3: Create feedback request for user confirmation
            var cache = feedback;
            feedback = new FeedbackRequest(
                prompt: $"Preview:\n{preview.Message}\nProceed with execution? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => executor(commandArgs), // Execute command with proper args
                        "no" => CommandResult.Fail("Execution cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no",
                            RequiresConfirmation = true,
                            Feedback = cache // reuse same feedback for retry
                        }
                    };
                });

            // Step 4: Return preview result (not final success yet)
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