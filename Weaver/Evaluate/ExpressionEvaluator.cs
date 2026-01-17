/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Evaluate
 * FILE:        ExpressionEvaluator.cs
 * PURPOSE:     Evaluates simple expressions (numeric, logical, registry-aware).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using System.Text.RegularExpressions;
using Weaver.Interfaces;
using Weaver.Messages;
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

        private static readonly string[] MultiOps =
        {
            "==", "!=", ">=", "<="
        };

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

            expression = expression.Trim();

            // direct boolean literals
            if (expression.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (expression.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;

            // try to interpret as a variable
            if (_registry != null && _registry.TryGet(expression, out var vm))
            {
                return vm.Type switch
                {
                    EnumTypes.Wbool => vm.Bool,
                    EnumTypes.Wint => vm.Int64 != 0,
                    EnumTypes.Wdouble => vm.Double != 0.0,
                    EnumTypes.Wstring => !string.IsNullOrEmpty(vm.String),
                    _ => throw new ArgumentException($"Unsupported variable type: {vm.Type}")
                };
            }

            // Unary NOT (handle both "not x" and "not(x)")
            if (expression.StartsWith("not", StringComparison.OrdinalIgnoreCase))
            {
                var remainder = expression.Substring(3).TrimStart();
                if (remainder.StartsWith("(") && remainder.EndsWith(")"))
                    remainder = remainder.Substring(1, remainder.Length - 2);

                return !Evaluate(remainder);
            }

            // --- NEW FIX: detect boolean operators safely ---
            var boolRegex = new Regex(@"\b(and|or)\b|&&|\|\|", RegexOptions.IgnoreCase);

            if (boolRegex.IsMatch(expression))
            {
                // split but KEEP operators
                var tokens = boolRegex.Split(expression)
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

                if (tokens.Length == 3)
                {
                    var left = Evaluate(tokens[0]);
                    var op = tokens[1].ToLowerInvariant();
                    var right = Evaluate(tokens[2]);

                    return op switch
                    {
                        "and" => left && right,
                        "or" => left || right,
                        "&&" => left && right,
                        "||" => left || right,
                        _ => throw new ArgumentException($"Unsupported logical operator: {op}")
                    };
                }
            }

            // --- comparison operators ---
            var parts = Tokenize(expression).ToArray();

            if (parts.Length == 3)
            {
                var left = GetValue(parts[0]);
                var op = parts[1];
                var right = GetValue(parts[2]);

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

            // unary NOT with tokens
            if (parts.Length == 2 && parts[0].Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                var val = GetValue(parts[1]);
                return !Convert.ToBoolean(val);
            }

            // fallback numeric
            return EvaluateNumeric(expression) != 0;
        }

        /// <inheritdoc />
        public double EvaluateNumeric(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ToRpn(tokens);
            return EvaluateRpn(rpn);
        }
        private IEnumerable<string> Tokenize(string expr)
        {
            var token = new StringBuilder();
            int i = 0;

            while (i < expr.Length)
            {
                char c = expr[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // numeric or identifier
                if (char.IsLetterOrDigit(c) || c == '.')
                {
                    token.Append(c);
                    i++;
                    continue;
                }

                // Flush current token
                if (token.Length > 0)
                {
                    yield return token.ToString();
                    token.Clear();
                }

                // Multi-char operator detection
                bool matchedMulti = false;
                foreach (var op in MultiOps)
                {
                    if (expr.AsSpan(i).StartsWith(op))
                    {
                        yield return op;
                        i += op.Length;
                        matchedMulti = true;
                        break;
                    }
                }
                if (matchedMulti)
                    continue;

                // Single char operator
                yield return c.ToString();
                i++;
            }

            if (token.Length > 0)
                yield return token.ToString();
        }

        private List<string> ToRpn(IEnumerable<string> tokens)
        {
            var output = new List<string>();
            var ops = new Stack<string>();

            int Precedence(string op) => op switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                _ => 0
            };

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _) || IsNumericVariable(token))
                {
                    output.Add(token);
                }
                else if ("+-*/".Contains(token))
                {
                    while (ops.Count > 0 && Precedence(ops.Peek()) >= Precedence(token))
                        output.Add(ops.Pop());
                    ops.Push(token);
                }
                else if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    while (ops.Peek() != "(")
                        output.Add(ops.Pop());
                    ops.Pop();
                }
            }

            while (ops.Count > 0)
                output.Add(ops.Pop());

            return output;
        }

        private double EvaluateRpn(List<string> rpn)
        {
            var stack = new Stack<double>();

            foreach (var token in rpn)
            {
                if (double.TryParse(token, out var num))
                {
                    stack.Push(num);
                }
                else if (IsNumericVariable(token))
                {
                    stack.Push(GetNumericValue(token)); // lookup registry
                }
                else
                {
                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        _ => throw new Exception("Unknown operator")
                    });
                }
            }

            return stack.Pop();
        }

        private bool IsNumericVariable(string token)
        {
            if (_registry == null)
                return false;

            return _registry.TryGet(token, out var val, out var type)
                   && (type == EnumTypes.Wint || type == EnumTypes.Wdouble || type == EnumTypes.Wbool);
        }

        private double GetNumericValue(string token)
        {
            if (_registry != null && _registry.TryGet(token, out var val, out var type))
            {
                return val switch
                {
                    int i => i,
                    long l => l,
                    double d => d,
                    float f => f,
                    bool b => b ? 1 : 0,
                    _ => throw new InvalidOperationException($"Cannot convert value of type {val.GetType()} to number")
                };
            }

            if (double.TryParse(token, out var num))
                return num;

            throw new InvalidOperationException($"Invalid numeric token: {token}");
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