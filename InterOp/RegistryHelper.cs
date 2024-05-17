/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterOp
 * FILE:        InterOp/RegistryHelper.cs
 * PURPOSE:     The usual error Logging
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://edi.wang/post/2019/3/4/read-and-write-windows-registry-in-net-core
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace InterOp
{
    /// <summary>
    ///     Description of RegistryUtility.
    /// </summary>
    internal static class RegistryHelper
    {
        /// <summary>
        ///     Read registry from Custom Path
        /// </summary>
        /// <param name="registryPath">User specified Path</param>
        /// <returns>Dictionary of RegistryObjects</returns>
        internal static Dictionary<int, object> GetRegistryObjects(string registryPath)
        {
            var registry = new Dictionary<int, object>();
            var i = -1;

            using var key = Registry.LocalMachine.OpenSubKey(registryPath);
            try
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey == null)
                        {
                            Trace.WriteLine(InterOpResources.KeyError);
                            continue;
                        }

                        i++;
                        registry.Add(i, subKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }

            return registry;
        }

        /// <summary>
        ///     Sets the registry objects.
        /// </summary>
        /// <param name="registryPath">The registry path.</param>
        /// <param name="value">The value.</param>
        /// <returns>Success Status</returns>
        internal static bool SetRegistryObjects(string registryPath, KeyValuePair<string, string> value)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(registryPath);
                key?.SetValue(value.Key, value.Value);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
