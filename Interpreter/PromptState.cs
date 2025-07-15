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
    /// <summary>
    ///     internal State of the Prompt
    /// </summary>
    public enum PromptState
    {
        /// <summary>
        ///     The normal
        /// </summary>
        Normal = 0,

        /// <summary>
        ///     The waiting for feedback
        /// </summary>
        WaitingForFeedback = 1
    }
}
