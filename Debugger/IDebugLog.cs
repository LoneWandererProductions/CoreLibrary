/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ErCode.cs
 * PURPOSE:     Interface for the Debugging
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Debugger;

/// <summary>
///     The IDebugLog interface.
/// </summary>
internal interface IDebugLog
{
    /// <summary>
    ///     Start.
    /// </summary>
    void Start();

    /// <summary>
    ///     Start the window.
    /// </summary>
    void StartWindow();

    /// <summary>
    ///     Stop the debugging.
    /// </summary>
    void StopDebugging();
}
