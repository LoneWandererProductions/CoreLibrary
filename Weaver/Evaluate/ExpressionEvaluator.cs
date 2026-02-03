/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Evaluate
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Evaluates simple expressions (numeric, logical, registry-aware).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Registry;
using Weaver.ScriptEngine;

//TODO needs rework to support more complex expressions, parentheses, operator precedence, etc.

namespace Weaver.Evaluate
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

        private readonly RpnEngine _rpn;

        /// <summary>
        /// The operators
        /// </summary>
        private static readonly Dictionary<string, (int precedence, bool rightAssociative, int arity)> Operators = new()
        {
            ["!"] = (5, true, 1),

            ["*"] = (4, false, 2),
            ["/"] = (4, false, 2),

            ["+"] = (3, false, 2),
            ["-"] = (3, false, 2),

            [">"] = (2, false, 2),
            ["<"] = (2, false, 2),
            [">="] = (2, false, 2),
            ["<="] = (2, false, 2),
            ["=="] = (2, false, 2),
            ["!="] = (2, false, 2),

            ["&&"] = (1, false, 2),
            ["||"] = (0, false, 2),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluator"/> class.
        /// </summary>
        /// <param name="registry">Optional variable registry.</param>
        public ExpressionEvaluator(IVariableRegistry? registry = null)
        {
            _registry = registry;
            _rpn = new RpnEngine(registry);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">Invalid or unsupported expression.</exception>
        public bool Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Expression cannot be empty.", nameof(expression));

            expression = expression.Trim();

            // direct boolean literals
            if (expression.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (expression.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // single-variable evaluation
            if (_registry != null && _registry.TryEvaluateAsBool(expression, out var single))
                return single;

            // try to interpret as a variable
            if (_registry != null) expression = _registry.ReplaceVariablesInExpression(expression);

            // --- comparison operators ---
            var tokens = Lexer.Tokenize(expression).ToArray();

            if (tokens.Length == 2 && tokens[0] == ScriptConstants.LogicalNotSymbol) // "!"
            {
                var val = GetValue(tokens[1]);
                return !Convert.ToBoolean(val);
            }

            if (tokens.Length == 3)
            {
                var left = GetValue(tokens[0]);
                var op = tokens[1];
                var right = GetValue(tokens[2]);

                return op switch
                {
                    ScriptConstants.EqualEqual => Equals(left, right),
                    ScriptConstants.BangEqual => !Equals(left, right),
                    ScriptConstants.Greater => Compare(left, right) > 0,
                    ScriptConstants.Less => Compare(left, right) < 0,
                    ScriptConstants.GreaterEqual => Compare(left, right) >= 0,
                    ScriptConstants.LessEqual => Compare(left, right) <= 0,
                    ScriptConstants.LogicalAndSymbol => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                    ScriptConstants.LogicalOrSymbol => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                    _ => throw new ArgumentException($"Unsupported operator: {op}")
                };
            }

            // unary NOT with tokens
            if (tokens.Length == 2 && tokens[0].Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                var val = GetValue(tokens[1]);
                return !Convert.ToBoolean(val);
            }

            //TODO cache Tokens -> RPN conversion for performance

            // fallback numeric
            return EvaluateNumeric(expression) != 0;
        }

        /// <inheritdoc />
        public double EvaluateNumeric(string expression)
        {
            //var tokens = Tokenizer.Tokenize(expression);

            var tokens = Lexer.Tokenize(expression);
            //var rpn = ToRpn(tokens);
            //return EvaluateRpn(rpn);

            return _rpn.EvaluateRpn(tokens);
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
            return expression.Contains(ScriptConstants.LogicalAndSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOrSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNotSymbol, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalAndWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalOrWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.LogicalNotWord, StringComparison.OrdinalIgnoreCase)
                   || expression.Contains(ScriptConstants.EqualEqual)
                   || expression.Contains(ScriptConstants.BangEqual)
                   || expression.Contains(ScriptConstants.GreaterEqual)
                   || expression.Contains(ScriptConstants.LessEqual)
                   || expression.Contains(ScriptConstants.Greater)
                   || expression.Contains(ScriptConstants.Less);
        }
    }
}