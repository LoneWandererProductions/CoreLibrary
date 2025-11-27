/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        ICommandExtension.cs
 * PURPOSE:     Interface for evaluators used in the Scriptengine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Interfaces
{
    /// <summary>
    /// Interface for expression evaluators used in the Scriptengine.
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>If expressions evaluate true or false.</returns>
        bool Evaluate(string expression);

        /// <summary>
        /// Basic numeric evaluation (add/sub/mul/div) for simple arithmetic expressions.
        /// Could later integrate a proper expression parser.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Value as double.</returns>
        double EvaluateNumeric(string expression);

        /// <summary>
        /// Determines whether [is boolean expression] [the specified expression].
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///   <c>true</c> if [is boolean expression] [the specified expression]; otherwise, <c>false</c>.
        /// </returns>
        bool IsBooleanExpression(string expression);
    }
}