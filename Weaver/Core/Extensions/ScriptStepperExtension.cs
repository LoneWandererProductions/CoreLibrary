/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Extensions
 * FILE:        ScriptStepperExtension.cs
 * PURPOSE:     Step Extension for Weaver scripts and ScriptCommand.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core.Commands;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Weaver.Core.Extensions
{
    /// <inheritdoc />
    /// <summary>
    /// Extension to step through a Weaver script one instruction at a time.
    /// </summary>
    /// <seealso cref="ICommandExtension" />
    public sealed class ScriptStepperExtension : ICommandExtension
    {
        /// <summary>
        /// The registry
        /// </summary>
        private IVariableRegistry _registry;

        /// <inheritdoc />
        public string Name => "Step";

        /// <inheritdoc />
        public string Description => "Steps through a compiled Weaver script one instruction at a time.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        public ScriptStepperExtension(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public CommandResult Invoke(
            ICommand command,
            string[] extensionArgs,
            Func<string[], CommandResult> executor,
            string[] commandArgs)
        {
            if (command is not ScriptCommand)
                return CommandResult.Fail("Script.Step extension can only be used with Script() command.");

            if (commandArgs.Length == 0)
                return CommandResult.Fail("Missing script text.");

            string script = commandArgs[0];

            WeaverProgram program;
            try
            {
                program = WeaverProgram.Compile(script, _registry);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail("Compile error: " + ex.Message);
            }

            var weave = new Weave();
            var stepper = program.GetStepper(weave);

            if (stepper.IsFinished)
                return CommandResult.Ok("Script finished.");

            try
            {
                stepper.ExecuteNext();
            }
            catch (Exception ex)
            {
                return CommandResult.Fail("Step error: " + ex.Message);
            }

            return CommandResult.Ok($"Executed step {stepper.InstructionPointer}.");
        }
    }
}
