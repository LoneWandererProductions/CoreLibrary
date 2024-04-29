using System;
using System.Collections.Generic;

namespace CommonFilter
{
    public static class LogicEvaluations
    {
        public static bool Evaluate(string inputString, IEnumerable<(string Operator, string Text, string LogicalOperator)> conditions)
        {
            var result = true;

            foreach (var condition in conditions)
            {
                bool conditionResult;

                switch (condition.Operator.ToLower())
                {
                    case "like":
                        conditionResult = inputString.Contains(condition.Text);
                        break;
                    case "not like":
                        conditionResult = !inputString.Contains(condition.Text);
                        break;
                    // Handle additional operators if needed
                    default:
                        throw new ArgumentException($"Unsupported operator: {condition.Operator}");
                }

                if (condition.LogicalOperator.ToLower() == "and")
                    result = result && conditionResult;
                else if (condition.LogicalOperator.ToLower() == "or")
                    result = result || conditionResult;
                else
                    throw new ArgumentException($"Unsupported logical operator: {condition.LogicalOperator}");
            }

            return result;
        }
    }
}
