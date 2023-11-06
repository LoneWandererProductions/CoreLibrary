/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/PluginLoaderResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

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
        internal const string FileExt = ".dll";

        /// <summary>
        ///     The separator
        /// </summary>
        internal const string Separator = ",";

        /// <summary>
        ///     The error could not find plugin
        /// </summary>
        internal const string ErrorCouldNotFindPlugin = "Can't find any type which implements ICommand in";

        /// <summary>
        ///     The message types
        /// </summary>
        internal const string MessageTypes = "Available types:";
    }
}
