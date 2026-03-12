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

        /// <summary>
        /// The RPN
        /// </summary>
        private readonly RpnEngine _rpn;

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

            // 1. Fast Path: Direct boolean literals
            if (expression.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (expression.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;

            // 2. Fast Path: Single-variable evaluation
            if (_registry != null && _registry.TryEvaluateAsBool(expression, out var single))
                return single;

            // 3. Complex Evaluation
            // We NO LONGER do string replacement here! 
            // The Lexer safely splits the string, and the RPN Engine fetches variables from the registry.
            var tokens = Lexer.Tokenize(expression).ToList();

            // RpnEngine calculates the entire math/logic tree and returns a double (1 or 0 for logic)
            var numericResult = _rpn.EvaluateRpn(tokens);

            // Convert C-style numeric boolean back to C# bool
            return numericResult != 0;
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
