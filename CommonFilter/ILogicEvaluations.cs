/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/LogicEvaluations.cs
 * PURPOSE:     Interface for LogicEvaluations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace CommonFilter
{
    /// <summary>
    ///     Interface for complex evaluations
    /// </summary>
    public interface ILogicEvaluations
    {
        /// <summary>
        ///     Evaluates the specified input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>If conditions are met</returns>
        bool Evaluate(string inputString, List<FilterOption> conditions);
    }
}
