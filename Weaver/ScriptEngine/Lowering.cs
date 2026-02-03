/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lowering.cs
 * PURPOSE:     Converts the ScriptNode tree into a linear sequence for execution.
 *              Optional but recommended it replaces some syntax sugar into correctly executable commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

//TODO the node exit for DoWhile is currently not implemented in the ScriptExecutor

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

                        if (rewrite ?? true)
                        {
                            var rewrittenExpr = expr;

                            // Replace registry variables if possible
                            if (registry != null)
                                rewrittenExpr = ReplaceRegistryVariables(expr, registry);

                            if (IsCommandCall(expr))
                            {
                                yield return (ScriptConstants.CommandRewriteToken, $"{expr}.Store({varName})");
                            }
                            else if (IsSimpleExpression(expr))
                            {
                                yield return (ScriptConstants.CommandRewriteToken,
                                    $"EvaluateCommand({rewrittenExpr}, {varName})");
                            }
                            else
                            {
                                throw new Exception($"Unsupported assignment expression: '{expr}'");
                            }
                        }
                        else
                        {
                            yield return (ScriptConstants.AssignmentToken, $"{varName} = {expr}");
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
            foreach (var kv in registry.GetAll())
            {
                var name = kv.Key;
                var vm = kv.Value;

                var replacement = vm.Type switch
                {
                    EnumTypes.Wint => vm.Int64.ToString(),
                    EnumTypes.Wdouble => vm.Double.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    EnumTypes.Wbool => vm.Bool ? "1" : "0",
                    EnumTypes.Wstring => $"\"{vm.String}\"",
                    _ => throw new InvalidOperationException($"Unsupported EnumType {vm.Type}")
                };

                // Replace whole-word occurrences only to avoid partial matches
                expr = Regex.Replace(expr, $@"\b{name}\b", replacement);
            }

            return expr;
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
            if (expr.Contains("(") || expr.Contains(")"))
                return false;

            foreach (var c in expr)
                if (!char.IsLetterOrDigit(c) && "+-*/<>=!&| ".IndexOf(c) < 0)
                    return false;

            return true;
        }
    }
}