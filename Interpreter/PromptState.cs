/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/PromptState.cs
 * PURPOSE:     Handle the the state of the input manager
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace Interpreter
{
    public enum PromptState
    {
        Normal,
        WaitingForFeedback
    }
}
