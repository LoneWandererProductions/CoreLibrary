/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        WeaveRuntime.cs
 * PURPOSE:     Encapsulates the execution state (variables, memory, evaluator)
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Evaluate; // Add this namespace
using Weaver.Interfaces;
using Weaver.Registry;

namespace Weaver
{
    /// <summary>
    /// The Weave runtime environment.
    /// Encapsulates the execution state (variables, memory) and evaluation logic.
    /// </summary>
    public sealed class WeaveRuntime
    {
        /// <summary>
        /// Gets the active variable registry.
        /// </summary>
        public IVariableRegistry Variables { get; }

        /// <summary>
        /// Gets the expression evaluator linked to this runtime.
        /// </summary>
        public IEvaluator Evaluator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaveRuntime"/> class.
        /// </summary>
        /// <param name="registry">Optional: A specific variable registry to use.</param>
        /// <param name="evaluator">Optional: A specific evaluator to use.</param>
        public WeaveRuntime(IVariableRegistry? registry = null, IEvaluator? evaluator = null)
        {
            // 1. Setup Variables
            Variables = registry ?? new VariableRegistry();

            // 2. Setup Evaluator (linked to the variables)
            Evaluator = evaluator ?? new ExpressionEvaluator(Variables);
        }
    }
}