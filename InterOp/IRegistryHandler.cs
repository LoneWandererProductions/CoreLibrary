/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterOp
 * FILE:        InterOp/IRegistryHandler.cs
 * PURPOSE:     Registry Reader Interface
 * PROGRAMER:   Peter Geinitz
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;

namespace InterOp;

/// <summary>
///     The Registry Reader Interface
/// </summary>
internal interface IRegistryHandler
{
    /// <summary>
    ///     Gets the registry objects.
    /// </summary>
    /// <param name="registryPath">The registry path.</param>
    /// <returns>All Registry Objects in a certain Path</returns>
    Dictionary<string, object> GetRegistryObjects(string registryPath);

    /// <summary>
    ///     Writes the registry.
    /// </summary>
    /// <param name="registryPath">The registry path.</param>
    /// <param name="value">The value.</param>
    /// <returns>Success Status</returns>
    bool WriteRegistry(string registryPath, KeyValuePair<string, string> value);
}
