/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ErCode.cs
 * PURPOSE:     Error Codes
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Debugger;

/// <summary>
///     Define all possible Error Types
/// </summary>
public enum ErCode
{
    /// <summary>
    ///     Diagnostic Message = 0.
    /// </summary>
    Diagnostic = 0,

    /// <summary>
    ///     Error Message = 1.
    /// </summary>
    Error = 1,

    /// <summary>
    ///     Warning Message= 2.
    /// </summary>
    Warning = 2,

    /// <summary>
    ///     Information Message = 3.
    /// </summary>
    Information = 3,

    /// <summary>
    ///     External Message = 4.
    /// </summary>
    External = 4
}
