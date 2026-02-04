/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        IPlugin.cs
 * PURPOSE:     Basic Plugin Support for some apps
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

// ReSharper disable UnusedParameter.Global, future proofing, it is up to the person how to use this ids
// ReSharper disable UnusedMember.Global

namespace Contracts
{
    /// <summary>
    ///     Plugin Interface Implementation
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        ///     Gets the name. This field must be equal to the file name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type. This field is optional.
        /// </summary>
        string Type { get; }

        /// <summary>
        ///     Gets the description. This field is optional.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets the version. This field is optional.
        /// </summary>
        Version Version { get; }

        /// <summary>
        ///     Executes this instance. Absolute necessary.
        /// </summary>
        /// <returns>Status Code</returns>
        int Execute();
    }
}
