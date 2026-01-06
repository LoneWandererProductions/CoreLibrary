/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptNode.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Collective base for all script nodes.
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    public abstract record ScriptNode(int Position);

    /// <summary>
    /// Position of label in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.LabelNode&gt;" />
    public sealed record LabelNode(int Position, string Name) : ScriptNode(Position);

    /// <summary>
    /// Position of goto in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.GotoNode&gt;" />
    public sealed record GotoNode(int Position, string Target) : ScriptNode(Position);

    /// <summary>
    /// Position of command in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.CommandNode&gt;" />
    public sealed record CommandNode(int Position, string Command) : ScriptNode(Position);

    /// <summary>
    /// Position of assignment in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.AssignmentNode&gt;" />
    public sealed record AssignmentNode(int Position, string Variable, string Expression) : ScriptNode(Position);

    /// <summary>
    /// Position of if statement in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.IfNode&gt;" />
    public sealed record IfNode(int Position, string Condition, List<ScriptNode> TrueBranch,
        List<ScriptNode>? FalseBranch = null) : ScriptNode(Position);

    /// <summary>
    /// Position of do-while loop in the script.
    /// </summary>
    /// <seealso cref="Weaver.ScriptEngine.ScriptNode" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.ScriptNode&gt;" />
    /// <seealso cref="System.IEquatable&lt;Weaver.ScriptEngine.DoWhileNode&gt;" />
    public sealed record DoWhileNode(int Position, List<ScriptNode> Body, string Condition) : ScriptNode(Position);
}