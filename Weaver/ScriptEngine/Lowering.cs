/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        Lowering.cs
 * PURPOSE:     Converts the ScriptNode tree into a linear sequence for execution
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
        public static IEnumerable<(string Category, string? Statement)> ScriptLowerer(IEnumerable<ScriptNode> nodes)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case LabelNode ln:
                        yield return ("Label", ln.Name);

                        break;

                    case GotoNode gn:
                        yield return ("Goto", gn.Target);

                        break;

                    case CommandNode cn:
                        yield return ("Command", cn.Command);

                        break;

                    case AssignmentNode an:
                        yield return ("Assignment", $"{an.Variable} = {an.Expression}");

                        break;

                    case IfNode ifn:
                        yield return ("If_Condition", ifn.Condition);
                        yield return ("If_Open", null); // debug open

                        foreach (var child in ScriptLowerer(ifn.TrueBranch))
                            yield return child;

                        yield return ("If_End", null); // debug end

                        if (ifn.FalseBranch != null)
                        {
                            yield return ("Else_Open", null);

                            foreach (var child in ScriptLowerer(ifn.FalseBranch))
                                yield return child;

                            yield return ("Else_End", null);
                        }

                        break;

                    case DoWhileNode dw:
                        yield return ("Do_Open", null);

                        foreach (var child in ScriptLowerer(dw.Body))
                            yield return child;

                        yield return ("Do_End", null);
                        yield return ("While_Condition", dw.Condition);

                        break;
                }
            }
        }
    }
}