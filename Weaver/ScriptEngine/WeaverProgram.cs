/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        WeaverProgram.cs
 * PURPOSE:     The compiler for Weaver scripts and runner.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Represents a compiled Weaver script ready for execution.
    /// </summary>
    public sealed class WeaverProgram
    {
        /// <summary>
        /// The instructions
        /// </summary>
        private readonly IEnumerable<(string Category, string? Statement)> _instructions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaverProgram"/> class.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <exception cref="System.ArgumentNullException">instructions</exception>
        private WeaverProgram(IEnumerable<(string Category, string? Statement)> instructions)
        {
            _instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
        }

        /// <summary>
        /// Compiles the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>The Script converted and ready for execution.</returns>
        /// <exception cref="System.ArgumentException">Script cannot be null or empty. - script</exception>
        public static WeaverProgram Compile(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentException("Script cannot be null or empty.", nameof(script));

            var lexer = new Lexer(script);
            var parser = new Parser(lexer.Tokenize());
            var nodes = parser.ParseIntoNodes();
            var instructions = Lowering.ScriptLowerer(nodes);

            return new WeaverProgram(instructions);
        }

        /// <summary>
        /// Runs the specified weave.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <param name="maxIterations">The maximum iterations.</param>
        /// <exception cref="System.ArgumentNullException">weave</exception>
        /// <exception cref="System.InvalidOperationException">Script execution exceeded maximum iterations. Possible infinite loop.</exception>
        public void Run(Weave weave, int maxIterations = 1000)
        {
            if (weave == null) throw new ArgumentNullException(nameof(weave));

            var executor = new ScriptExecutor(weave, _instructions
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList());

            int iteration = 0;

            while (!executor.IsFinished)
            {
                if (iteration++ > maxIterations)
                    throw new InvalidOperationException("Script execution exceeded maximum iterations. Possible infinite loop.");

                executor.ExecuteNext();
            }
        }

        /// <summary>
        /// Gets the stepper.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <returns>The Script Executor to rune the script.</returns>
        /// <exception cref="System.ArgumentNullException">weave</exception>
        public ScriptExecutor GetStepper(Weave weave)
        {
            if (weave == null) throw new ArgumentNullException(nameof(weave));
            return new ScriptExecutor(weave, _instructions
                .Where(line => line.Statement != null)
                .Select(line => (line.Category, line.Statement!))
                .ToList());
        }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <returns>The Converted insructions</returns>
        internal IEnumerable<(string Category, string? Statement)> GetInstructions() => _instructions;
    }

}
