/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lowering.cs
 * PURPOSE:     Converts the ScriptNode tree into a linear sequence for execution.
 *              Optional but recommended it replaces some syntax sugar into correctly executable commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text.RegularExpressions;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Script lowerer that flattens ScriptNode trees into linear sequences.
    /// </summary>
    internal static class Lowering
    {
        /// <summary>
        /// Flattens the ScriptNode tree into a linear sequence with debug categories.
        /// Includes pseudo-categories like Do_Open, Do_End, If_Open, If_End, Else_Open, Else_End.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="registry">Optional variable registry for early substitution.</param>
        /// <param name="rewrite">A debug switch for rewrite.</param>
        /// <param name="branchPath">Branch path for tracking nested structures.</param>
        /// <returns>Execution blocks.</returns>
        public static IEnumerable<(string Category, string? Statement)> ScriptLowerer(
            IEnumerable<ScriptNode> nodes,
            IVariableRegistry? registry = null,
            bool? rewrite = true,
            string branchPath = "")
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case LabelNode ln:
                        yield return (ScriptConstants.LabelToken, ln.Name);

                        break;

                    case GotoNode gn:
                        yield return (ScriptConstants.GotoToken, gn.Target);

                        break;

                    case CommandNode cn:
                        yield return (ScriptConstants.CommandToken, cn.Command);

                        break;

                    case AssignmentNode an:
                    {
                        var expr = an.Expression?.Trim() ?? "";
                        var varName = an.Variable?.Trim() ?? "";

                        // Step 1: Replace variables first so the following logic works on actual values
                        var processedExpr = expr;
                        if (registry != null)
                        {
                            processedExpr = ReplaceRegistryVariables(expr, registry);
                        }

                        if (rewrite ?? true)
                        {
                            // Step 2: Use processedExpr for logic and output
                            if (IsCommandCall(processedExpr))
                            {
                                yield return (ScriptConstants.CommandRewriteToken, $"{processedExpr}.Store({varName})");
                            }
                            else if (IsSimpleExpression(processedExpr))
                            {
                                // The Evaluator gets the expression with variables already swapped for literals
                                yield return (ScriptConstants.CommandRewriteToken,
                                    $"EvaluateCommand({processedExpr}, {varName})");
                            }
                            else
                            {
                                throw new Exception($"Unsupported assignment expression: '{processedExpr}'");
                            }
                        }
                        else
                        {
                            // Step 3: Even without structural rewrite, use the substituted expression
                            yield return (ScriptConstants.AssignmentToken, $"{varName} = {processedExpr}");
                        }

                        break;
                    }

                    case IfNode ifn:
                        // Emit condition
                        yield return (ScriptConstants.IfConditionToken, ifn.Condition);

                        // Open true branch
                        yield return (ScriptConstants.IfOpenToken, branchPath + "T");

                        foreach (var child in ScriptLowerer(ifn.TrueBranch, registry, rewrite, branchPath + "T"))
                            yield return child;

                        yield return (ScriptConstants.IfEndToken, branchPath + "T");

                        if (ifn.FalseBranch != null)
                        {
                            yield return (ScriptConstants.ElseOpenToken, branchPath + "F");

                            foreach (var child in ScriptLowerer(ifn.FalseBranch, registry, rewrite, branchPath + "F"))
                                yield return child;

                            yield return (ScriptConstants.ElseEndToken, branchPath + "F");
                        }

                        break;

                    case DoWhileNode dw:

                        yield return (ScriptConstants.DoOpenToken, null);

                        foreach (var child in ScriptLowerer(dw.Body, registry, rewrite, branchPath))
                            yield return child;

                        yield return (ScriptConstants.DoEndToken, null);
                        yield return (ScriptConstants.WhileConditionToken, dw.Condition);

                        break;
                }
            }
        }

        /// <summary>
        /// Replaces known registry variables in a simple expression with their literal values.
        /// </summary>
        private static string ReplaceRegistryVariables(string expr, IVariableRegistry registry)
        {
            var variables = registry.GetAll();
            if (variables == null || !variables.Any()) return expr;

            // 1. Build a pattern like: \b(var1|var2|var3)\b
            // Join all keys with | and wrap in word boundaries
            string pattern = $@"\b({string.Join("|", variables.Keys.Select(Regex.Escape))})\b";

            // 2. Use a single Regex.Replace with a callback (MatchEvaluator)
            return Regex.Replace(expr, pattern, match =>
            {
                var name = match.Value;
                if (!variables.TryGetValue(name, out var vm)) return name;

                return vm.Type switch
                {
                    EnumTypes.Wint => vm.Int64.ToString(),
                    EnumTypes.Wdouble => vm.Double.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    EnumTypes.Wbool => vm.Bool ? "1" : "0",
                    EnumTypes.Wstring => $"\"{vm.String}\"",
                    _ => throw new InvalidOperationException($"Unsupported EnumType {vm.Type}")
                };
            });
        }

        /// <summary>
        /// Determines whether [is command call] [the specified expr].
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>
        ///   <c>true</c> if [is command call] [the specified expr]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCommandCall(string expr)
        {
            var paren = expr.IndexOf('(');
            return paren > 0 && expr.EndsWith(")");
        }

        /// <summary>
        /// Determines whether [is simple expression] [the specified expr].
        /// </summary>
        /// <param name="expr">The expr.</param>
        /// <returns>
        ///   <c>true</c> if [is simple expression] [the specified expr]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSimpleExpression(string expr)
        {
            // Let the new RPN Engine handle the parentheses!
            foreach (var c in expr)
            {
                // Added '(' and ')' to the allowed characters
                if (!char.IsLetterOrDigit(c) && "+-*/<>=!&| ()".IndexOf(c) < 0)
                    return false;
            }

            return true;
        }
    }
}
