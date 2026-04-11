/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/LogicEvaluations.cs
 * PURPOSE:     An Implementation for ILogicEvaluations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace CommonFilter
{
    /// <inheritdoc />
    /// <summary>
    ///     Will be packed into an Interface and be an optional Interface for Filter
    /// </summary>
    public sealed class LogicEvaluations : ILogicEvaluations
    {
        /// <inheritdoc />
        /// <summary>
        ///     Evaluates the specified input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>If conditions are met</returns>
        /// <exception cref="T:System.ArgumentException">Unsupported operator</exception>
        public bool Evaluate(string inputString, List<FilterOption> conditions)
        {
            // Fallback for safety, though your Filter.cs already checks for 0 count
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            // 1. Establish the baseline using the very first condition.
            // (The logical operator of the first item is ignored because there is nothing before it to compare against)
            var result = EvaluateCondition(inputString, conditions[0]);

            // 2. Loop through the REST of the conditions and chain them together
            for (var i = 1; i < conditions.Count; i++)
            {
                var term = conditions[i];
                var conditionResult = EvaluateCondition(inputString, term);

                switch (term.SelectedLogicalOperator)
                {
                    case LogicOperator.And:
                        result = result && conditionResult;
                        break;
                    case LogicOperator.Or:
                        result = result || conditionResult;
                        break;
                    case LogicOperator.AndNot:
                        result = result && !conditionResult;
                        break;
                    case LogicOperator.OrNot:
                        result = result || !conditionResult;
                        break;
                    default:
                        throw new ArgumentException(string.Concat(FilterResources.ErrorLogicalOperator,
                            term.SelectedLogicalOperator));
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates a single condition against the input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="term">The term.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        private bool EvaluateCondition(string inputString, FilterOption term)
        {
            switch (term.SelectedCompareOperator)
            {
                case CompareOperator.Like:
                    return inputString.Contains(term.EntryText);
                case CompareOperator.NotLike:
                    return !inputString.Contains(term.EntryText);
                case CompareOperator.Equal:
                    return string.Equals(inputString, term.EntryText);
                case CompareOperator.NotEqual:
                    return !string.Equals(inputString, term.EntryText);
                default:
                    throw new ArgumentException(string.Concat(FilterResources.ErrorCompareOperator,
                        term.SelectedCompareOperator));
            }
        }
    }
}
