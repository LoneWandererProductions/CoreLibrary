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
        /// <exception cref="T:System.ArgumentException">Unsupported operator: {Operator}</exception>
        public bool Evaluate(string inputString, List<FilterOption> conditions)
        {
            var result = true;

            foreach (var term in conditions)
            {
                bool conditionResult;

                switch (term.SelectedCompareOperator)
                {
                    case CompareOperator.Like:
                        conditionResult = inputString.Contains(term.EntryText);
                        break;
                    case CompareOperator.NotLike:
                        conditionResult = !inputString.Contains(term.EntryText);
                        break;
                    case CompareOperator.Equal:
                        conditionResult = string.Equals(inputString, term.EntryText);
                        break;
                    case CompareOperator.NotEqual:
                        conditionResult = !string.Equals(inputString, term.EntryText);
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException(string.Concat(FilterResources.ErrorCompareOperator,
                            term.SelectedCompareOperator));
                }

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
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException(string.Concat(FilterResources.ErrorLogicalOperator,
                            term.SelectedLogicalOperator));
                }
            }

            return result;
        }
    }
}
