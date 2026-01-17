/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        ScriptCommand.cs
 * PURPOSE:     Provides a command wrapper to compile and execute Weaver scripts.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Weaver.Core.Commands
{
    /// <inheritdoc />
    /// <summary>
    /// Command to compile & execute Weaver scripts.
    /// </summary>
    internal class ScriptCommand : ICommand
    {
        /// <summary>
        /// The registry
        /// </summary>
        private IVariableRegistry _registry;

        /// <inheritdoc />
        public string Name => "RunScript";

        /// <inheritdoc />
        public string Description =>
            "Compiles and executes a Weaver script. Usage: RunScript(<scriptText> [, <maxIterations>])";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ParameterCount => -1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public ScriptCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public CommandResult Execute(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                return CommandResult.Fail("Missing script text. Usage: RunScript(<script> [, <maxIterations>])");
            }

            string script = args[0];
            int maxIterations = 1000;

            if (args.Length > 1 && int.TryParse(args[1], out var parsed))
                maxIterations = parsed;

            WeaverProgram program;

            try
            {
                program = WeaverProgram.Compile(script, _registry);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Compile error: {ex.Message}");
            }

            var weave = new Weave();

            try
            {
                program.Run(weave, maxIterations);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Runtime error: {ex.Message}");
            }

            return CommandResult.Ok("Script executed successfully.");
        }
    }
}
