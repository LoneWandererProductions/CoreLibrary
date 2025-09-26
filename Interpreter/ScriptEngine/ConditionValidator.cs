/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        ConditionValidator.cs
 * PURPOSE:     Validator for script conditions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Interpreter.ScriptEngine;

public class ConditionValidator
{
    public bool? LastConditionResult { get; private set; }
    public string? LastError { get; private set; }

    public void Attach(Prompt engine)
    {
        engine.SendCommands += OnCommandExecuted;
    }

    /// <summary>
    /// Called when [command executed].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="outCmd">The out command.</param>
    private void OnCommandExecuted(object sender, OutCommand outCmd)
    {
        // Only validate if this is part of a condition context
        if (IsConditionContext(outCmd))
        {
            if (!outCmd.IsSuccess)
            {
                LastError = outCmd.ErrorMessage;
                LastConditionResult = null;
                return;
            }

            if (outCmd.Result is bool b)
            {
                LastConditionResult = b;
                LastError = null;
            }
            else
            {
                LastError = $"Expected boolean result in condition, got: {outCmd.ActualReturnType?.Name ?? "null"}";
                LastConditionResult = null;
            }
        }
    }

    /// <summary>
    /// Determines whether [is condition context] [the specified command].
    /// </summary>
    /// <param name="cmd">The command.</param>
    /// <returns>
    ///   <c>true</c> if [is condition context] [the specified command]; otherwise, <c>false</c>.
    /// </returns>
    private bool IsConditionContext(OutCommand cmd)
    {
        // You define this logic:
        // Could be a naming convention like cmd.Command == Commands.CheckCondition
        return true; // for now, assume always true
    }
}
