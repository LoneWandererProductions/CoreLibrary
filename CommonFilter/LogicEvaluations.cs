/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/LogicEvaluations.cs
 * PURPOSE:     An Implementation for ILogicEvaluations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace CommonFilter
{
    /// <summary>
    ///     Will be packed into an Interface and be an optional Interface for Filter
    /// </summary>
    public class LogicEvaluations : ILogicEvaluations
    {
        /// <summary>
        /// Evaluates the specified input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>If conditions are met</returns>
        /// <exception cref="System.ArgumentException">Unsupported operator: {Operator}</exception>
        public bool Evaluate(string inputString, List<FilterOption> conditions)
        {
            var result = true;

            foreach (var term in conditions)
            {
                bool conditionResult;

                switch (term.SelectedCompareOperator)
                {
                    case CompareOperator.like:
                        conditionResult = inputString.Contains(term.EntryText);
                        break;
                    case CompareOperator.Notlike:
                        conditionResult = !inputString.Contains(term.EntryText);
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {term.SelectedCompareOperator}");
                }

                switch (term.SelectedLogicalOperator)
                {
                    case LogicOperator.and:
                        result = result && conditionResult;
                        break;
                    case LogicOperator.or:
                        result = result || conditionResult;
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {term.SelectedLogicalOperator}");
                }
            }

            return result;
        }
    }
}
