/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ILogSource
 * FILE:        Debugger/ILogSource.xaml.cs
 * PURPOSE:     Interface to make my log sources interchangeable
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace Debugger;

/// <summary>
/// Abstraction for a log source that can stream log lines.
/// </summary>
public interface ILogSource
{
    /// <summary>
    /// Raised whenever a new log line is received.
    /// </summary>
    event EventHandler<string> LineReceived;

    /// <summary>
    /// Returns all available log lines (e.g., from file or memory).
    /// </summary>
    IEnumerable<string> ReadAll();

    /// <summary>
    /// Start listening to the log source.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop listening to the log source.
    /// </summary>
    void Stop();
}
