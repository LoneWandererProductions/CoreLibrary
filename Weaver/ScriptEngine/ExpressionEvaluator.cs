/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Evaluates simple expressions (numeric, logical, registry-aware).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;

namespace Weaver.ScriptEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Evaluates simple logical or arithmetic expressions.
    /// Supports registry variables when available.
    /// </summary>
    internal sealed class ExpressionEvaluator : IEvaluator
    {
        /// <summary>
        /// Optional variable registry.
        /// </summary>
        private readonly IVariableRegistry? _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="registry">Optional variable registry.</param>
        public ExpressionEvaluator(IVariableRegistry? registry = null)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Invalid or unsupported expression.</exception>
        public bool Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be empty.", nameof(expression));

            // Tokenize for simple comparison (e.g., "x == 5")
            var tokens = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 3)
            {
                var left = GetValue(tokens[0]);
                var op = tokens[1];
                var right = GetValue(tokens[2]);

                // Convert to bool if logical operator
                if (op == ScriptConstants.LogicalAnd || op == ScriptConstants.LogicalOr)
                {
                    var leftBool = Convert.ToBoolean(left);
                    var rightBool = Convert.ToBoolean(right);

                    return op switch
                    {
                        ScriptConstants.LogicalAnd => leftBool && rightBool,
                        ScriptConstants.LogicalOr => leftBool || rightBool,
                        _ => throw new ArgumentException($"Unsupported operator: {op}")
                    };
                }

                return op switch
                {
                    ScriptConstants.EqualEqual => Equals(left, right),
                    ScriptConstants.BangEqual => !Equals(left, right),
                    ScriptConstants.Greater => Compare(left, right) > 0,
                    ScriptConstants.Less => Compare(left, right) < 0,
                    ScriptConstants.GreaterEqual => Compare(left, right) >= 0,
                    ScriptConstants.LessEqual => Compare(left, right) <= 0,
                    ScriptConstants.LogicalAnd => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                    ScriptConstants.LogicalOr => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                    _ => throw new ArgumentException($"Unsupported operator: {op}")
                };
            }

            // Unary NOT support (e.g., "not x")
            if (tokens.Length == 2 && tokens[0].Equals(ScriptConstants.LogicalNot, StringComparison.OrdinalIgnoreCase))
            {
                var val = GetValue(tokens[1]);
                return !Convert.ToBoolean(val);
            }

            // Fallback to numeric evaluation.
            return EvaluateNumeric(expression) != 0;
        }

        /// <inheritdoc />
        public double EvaluateNumeric(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be empty.", nameof(expression));

            var dt = new System.Data.DataTable();
            var value = dt.Compute(expression, string.Empty);
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Retrieve a token value (number, variable, or literal string).
        /// </summary>
        private object GetValue(string token)
        {
            // Try numeric literal
            if (double.TryParse(token, out var num))
                return num;

            // Try registry lookup
            if (_registry != null && _registry.TryGet(token, out var val, out _))
                return val!;

            // Treat as literal string
            return token;
        }

        /// <summary>
        /// Compare two numeric values.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>Compare result.</returns>
        private static int Compare(object a, object b)
        {
            var da = Convert.ToDouble(a);
            var db = Convert.ToDouble(b);
            return da.CompareTo(db);
        }

        /// <inheritdoc />
        public bool IsBooleanExpression(string expression)
        {
            return expression.Contains(ScriptConstants.LogicalAnd, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOr, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNot, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.EqualEqual)
                   || expression.Contains(ScriptConstants.BangEqual)
                   || expression.Contains(ScriptConstants.GreaterEqual)
                   || expression.Contains(ScriptConstants.LessEqual)
                   || expression.Contains(ScriptConstants.Greater)
                   || expression.Contains(ScriptConstants.Less);
        }
    }
}