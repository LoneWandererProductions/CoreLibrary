/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptNode.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    public abstract record ScriptNode(int Position);

    public sealed record LabelNode(int Position, string Name) : ScriptNode(Position);

    public sealed record GotoNode(int Position, string Target) : ScriptNode(Position);

    public sealed record CommandNode(int Position, string Command) : ScriptNode(Position);

    public sealed record AssignmentNode(int Position, string Variable, string Expression) : ScriptNode(Position);

    public sealed record IfNode(int Position, string Condition, List<ScriptNode> TrueBranch,
        List<ScriptNode>? FalseBranch = null) : ScriptNode(Position);

    public sealed record DoWhileNode(int Position, List<ScriptNode> Body, string Condition) : ScriptNode(Position);
}