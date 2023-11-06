/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/IPlugin.cs
 * PURPOSE:     Basic Plugin Support
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

using System;

namespace Plugin
{
    /// <summary>
    ///     Plugin Interface Implementation
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        string Name { get; }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        Version Version { get; }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>Status Code</returns>
        int Execute();

        /// <summary>
        ///     Closes this instance.
        /// </summary>
        /// <returns>Status Code</returns>
        int Close();
    }
}
