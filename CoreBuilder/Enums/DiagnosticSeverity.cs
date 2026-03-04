/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Enums
 * FILE:        DiagnosticSeverity.cs
 * PURPOSE:     Serverity of our diagnostic message, probably mostly Infos, Warnings and Errors are rare.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder.Enums
{
    /// <summary>
    /// Severity of the diagnostic message.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>
        /// The information
        /// </summary>
        Info = 0,

        /// <summary>
        /// The warning
        /// </summary>
        Warning = 1,

        /// <summary>
        /// The error
        /// </summary>
        Error = 2
    }
}