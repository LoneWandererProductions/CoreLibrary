/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterOp
 * FILE:        InterOp/RegistryHandler.cs
 * PURPOSE:     Implementation of IRegistryReader
 * PROGRAMER:   Peter Geinitz
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace InterOp
{
    /// <inheritdoc />
    /// <summary>
    ///     Registry Reader
    /// </summary>
    /// <seealso cref="RegistryHandler" />
    public sealed class RegistryHandler : IRegistryHandler
    {
        /// <inheritdoc />
        /// <summary>
        ///     Gets the registry objects.
        /// </summary>
        /// <param name="registryPath">The registry path.</param>
        /// <returns>
        ///     All Registry Objects in a certain Path
        /// </returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public Dictionary<int, object> GetRegistryObjects(string registryPath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }

            return RegistryHelper.GetRegistryObjects(registryPath);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Writes the registry.
        /// </summary>
        /// <param name="registryPath">The registry path.</param>
        /// <param name="value">The value.</param>
        /// <returns>Success Status</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public bool WriteRegistry(string registryPath, KeyValuePair<string, string> value)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }

            return RegistryHelper.SetRegistryObjects(registryPath, value);
        }
    }
}
