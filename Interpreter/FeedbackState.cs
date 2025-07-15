/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        FeedbackState.cs
 * PURPOSE:     Internal States fo the Feedbackmanager
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Interpreter
{
    /// <summary>
    ///     State of the Feedbackmanager
    /// </summary>
    public enum FeedbackState
    {
        /// <summary>
        ///     The idle
        /// </summary>
        Idle = 0,

        /// <summary>
        ///     The waiting
        /// </summary>
        Waiting = 1
    }
}
