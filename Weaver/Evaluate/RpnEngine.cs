/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Evaluate
 * FILE:        RpnEngine.cs
 * PURPOSE:     Implementation of RPN Engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Registry;
using Weaver.ScriptEngine;

namespace Weaver.Evaluate
{
    /// <inheritdoc />
    /// <summary>
    /// Main implementation of a Reverse Polish Notation (RPN) evaluation engine.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.IRpnEngine" />
    public class RpnEngine : IRpnEngine
    {
        /// <summary>
        /// The registry
        /// </summary>
        private IVariableRegistry? _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="RpnEngine"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public RpnEngine(IVariableRegistry? registry = null)
        {
            _registry = registry;
        }

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


        /// <inheritdoc />
        public double EvaluateRpn(List<string> tokens)
        {
            var rpn = ToRpn(tokens);
            return Evaluate(rpn);
        }

        /// <summary>
        /// Converts to rpn.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>Reverse Polish Notation expfression.</returns>
        private List<string> ToRpn(IEnumerable<string> tokens)
        {
            var output = new List<string>();
            var ops = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _) || _registry!.IsNumericType(token))
                {
                    output.Add(token);
                    continue;
                }

                if (token == "(")
                {
                    ops.Push(token);
                    continue;
                }

                if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                        output.Add(ops.Pop());

                    ops.Pop(); // remove "("
                    continue;
                }

                if (!Operators.TryGetValue(token, out var op1))
                    continue;

                while (ops.Count > 0 && Operators.TryGetValue(ops.Peek(), out var op2))
                {
                    if ((op1.rightAssociative && op1.precedence < op2.precedence) ||
                        (!op1.rightAssociative && op1.precedence <= op2.precedence))
                    {
                        output.Add(ops.Pop());
                    }
                    else break;
                }

                ops.Push(token);
            }

            while (ops.Count > 0)
                output.Add(ops.Pop());

            return output;
        }

        /// <summary>
        /// Evaluates the specified RPN.
        /// </summary>
        /// <param name="rpn">The RPN.</param>
        /// <returns>Result of evaluation 1 or 0.</returns>
        private double Evaluate(List<string> rpn)
        {
            var stack = new Stack<double>();

            foreach (var token in rpn)
            {
                if (double.TryParse(token, out var num))
                {
                    stack.Push(num);
                }
                else if (_registry!.IsNumericType(token))
                {
                    stack.Push(GetNumericValue(token));
                }
                else if (token == ScriptConstants.LogicalNotSymbol) // "!"
                {
                    var val = stack.Pop();
                    stack.Push(val == 0 ? 1 : 0);
                }
                else
                {
                    // binary operators
                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        "&&" => (a != 0 && b != 0) ? 1 : 0,
                        "||" => (a != 0 || b != 0) ? 1 : 0,
                        "==" => a == b ? 1 : 0,
                        "!=" => a != b ? 1 : 0,
                        ">" => a > b ? 1 : 0,
                        "<" => a < b ? 1 : 0,
                        ">=" => a >= b ? 1 : 0,
                        "<=" => a <= b ? 1 : 0,
                        _ => throw new Exception($"Unknown operator in RPN: {token}")
                    });
                }
            }

            return stack.Pop();
        }

        /// <summary>
        /// Gets the numeric value.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Registry value as numeric value</returns>
        /// <exception cref="System.InvalidOperationException">Invalid numeric token: {token}</exception>
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
    }
}