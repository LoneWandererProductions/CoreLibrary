using System;
using System.Collections.Generic;

namespace CommonFilter
{
    public static class LogicEvaluations
    {
        public static bool Evaluate(string inputString, IEnumerable<(string Operator, string Text, string LogicalOperator)> conditions)
        {
            var result = true;

            foreach (var (Operator, Text, LogicalOperator) in conditions)
            {
                bool conditionResult;

                switch (Operator.ToLower())
                {
                    case "like":
                        conditionResult = inputString.Contains(Text);
                        break;
                    case "not like":
                        conditionResult = !inputString.Contains(Text);
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {Operator}");
                }

                if (string.Equals(LogicalOperator, "and", StringComparison.OrdinalIgnoreCase))
                    result = result && conditionResult;
                else if (string.Equals(LogicalOperator, "or", StringComparison.OrdinalIgnoreCase))
                    result = result || conditionResult;
                else
                    throw new ArgumentException($"Unsupported logical operator: {LogicalOperator}");
            }

            return result;
        }
    }
}
