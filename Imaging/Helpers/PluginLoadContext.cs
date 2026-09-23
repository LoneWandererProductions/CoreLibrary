/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Helpers
 * FILE:        PluginLoadContext.cs
 * PURPOSE:     Plugin Loader for advanced plugins. Resolves dependencies of the plugin to the plugin's folder, and loads shared assemblies from the default context.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Imaging.Helpers
{
    /// <inheritdoc />
    /// <summary>
    /// A loader for Plugins.
    /// Resolves dependencies of the plugin to the plugin's folder, and loads shared assemblies from the default context.
    /// </summary>
    /// <seealso cref="AssemblyLoadContext" />
    public sealed class PluginLoadContext : AssemblyLoadContext
    {
        /// <summary>
        /// The plugin path
        /// </summary>
        private readonly string _pluginPath;

        /// <summary>
        /// The resolver
        /// </summary>
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
            : base(
                name: $"Plugin:{Path.GetFileNameWithoutExtension(pluginPath)}",
                isCollectible: false)
        {
            _pluginPath = pluginPath;
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        /// <inheritdoc />
        protected override Assembly? Load(
            AssemblyName assemblyName)
        {
            // These assemblies define the types shared between
            // the host and the plugin. They must come from the
            // default context.

            if (assemblyName.Name is
                "Imaging.Plugins.Interface" or
                "System.Drawing.Common")
            {
                return Default
                    .LoadFromAssemblyName(assemblyName);
            }

            var assemblyPath =
                _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        /// <inheritdoc />
        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            Trace.WriteLine(
                $"[PluginLoadContext] Native dependency requested: {unmanagedDllName}");

            Trace.WriteLine(
                $"[PluginLoadContext] Plugin path: {_pluginPath}");

            var libraryPath =
                _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            Trace.WriteLine(
                $"[PluginLoadContext] Native dependency resolved to: {libraryPath ?? "<not found>"}");

            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return nint.Zero;
        }
    }
}
