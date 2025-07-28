/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreWorker
 * FILE:        CoreWorker/CoreBackgroundWorker.cs
 * PURPOSE:     Interface of CoreBackgroundWorker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CoreWorker;

/// <summary>
///     Interface what we expect
/// </summary>
public interface ICoreBackgroundWorker
{
    /// <summary>
    ///     Starts this instance.
    /// </summary>
    void Start();

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    void Stop();
}
