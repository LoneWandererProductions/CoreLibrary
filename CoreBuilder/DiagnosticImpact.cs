/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Diagnostic.cs
 * PURPOSE:     Class representing a diagnostic result.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder
{
    /// <summary>
    /// Diagnostic Impact
    /// </summary>
    public enum DiagnosticImpact
    {
        /// <summary>
        /// high CPU usage, repeated calculations, tight loops
        /// </summary>
        CpuBound = 0,
        /// <summary>
        /// file/network/database operations
        /// </summary>
        IoBound = 1,

        /// <summary>
        /// artificial waits like Task.Delay
        /// </summary>
        Throttling = 2,

        /// <summary>
        /// miscellaneous, low-risk
        /// </summary>
        Other = 3
    }
}
