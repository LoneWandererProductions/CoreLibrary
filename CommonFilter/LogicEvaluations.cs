using System;
using System.Collections.Generic;

namespace CommonFilter
{
    public static class LogicEvaluations
    {
        public static bool Evaluate(string inputString, IEnumerable<(OptionsOperator Operator, string Text, LogicOperator LogicalOperator)> conditions)
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
                }
            }

            return result;
        }
    }
}
