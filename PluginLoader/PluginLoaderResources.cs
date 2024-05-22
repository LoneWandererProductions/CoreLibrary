/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        PluginLoader/PluginLoaderResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

using System;
using System.Reflection;

namespace PluginLoader
{
    /// <summary>
    ///     String Resources
    /// </summary>
    internal static class PluginLoaderResources
    {
        /// <summary>
        ///     The file ext
        /// </summary>
        internal const string FileExt = "*.dll";

        /// <summary>
        ///     The separator
        /// </summary>
        internal const string Separator = ",";

        /// <summary>
        ///     The error could not find plugin
        /// </summary>
        internal const string ErrorCouldNotFindPlugin = "Can't find any type which implements ICommand in: ";

        /// <summary>
        ///     The Error with the Path (const). Value: "Plugin path does not exist.".
        /// </summary>
        internal const string ErrorPath = "Plugin path does not exist.";

        /// <summary>
        ///     The Error Directory did not exist (const). Value: "Directory does not exist.".
        /// </summary>
        internal const string ErrorDirectory = "Directory does not exist.";

        /// <summary>
        ///     Information about Plugin Status (const). Value: "No Plugins found.".
        /// </summary>
        internal const string InformationPlugin = "No Plugins found.";

        /// <summary>
        ///     Format Information about the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="availableTypes">The available types.</param>
        /// <returns>Information about the assembly</returns>
        internal static string Information(Assembly assembly, string availableTypes)
        {
            return string.Concat($" {assembly} from {assembly.Location}.",
                Environment.NewLine, "Available types:",
                $" {availableTypes}");
        }
    }
}
