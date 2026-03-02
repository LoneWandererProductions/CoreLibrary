/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        StepType.cs
 * PURPOSE:     StepType enumeration for script execution debugs. For now only used internally.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Step types for script execution debugging.
    /// </summary>
    internal enum StepType
    {
        /// <summary>
        /// The input
        /// </summary>
        Input = 0,

        /// <summary>
        /// The execute
        /// </summary>
        Execute = 1,

        /// <summary>
        /// The output
        /// </summary>
        Output = 2,

        /// <summary>
        /// The internal
        /// </summary>
        Internal = 3
    }
}