using System;
using System.Collections.Generic;

namespace CommonFilter
{
    /// <summary>
    ///     Will be packed into an Interface and be an optional Interface for Filter
    /// </summary>
    public static class LogicEvaluations
    {
        /// <summary>
        ///     Evaluates the specified input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="conditions">The conditions.</param>
        /// <returns>Check a set of Conditions</returns>
        /// <exception cref="System.ArgumentException">Unsupported operator: {Operator}</exception>
        public static bool Evaluate(string inputString,
            IEnumerable<(OptionsOperator Operator, string Text, LogicOperator LogicalOperator)> conditions)
        {
            var result = true;

            foreach (var (Operator, Text, LogicalOperator) in conditions)
            {
                bool conditionResult;

                switch (Operator)
                {
                    case OptionsOperator.like:
                        conditionResult = inputString.Contains(Text);
                        break;
                    case OptionsOperator.Notlike:
                        conditionResult = !inputString.Contains(Text);
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {Operator}");
                }

                switch (LogicalOperator)
                {
                    case LogicOperator.and:
                        result = result && conditionResult;
                        break;
                    case LogicOperator.or:
                        result = result || conditionResult;
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {Operator}");
                }
            }

            return result;
        }
    }
}
