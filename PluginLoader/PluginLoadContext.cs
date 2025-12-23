/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PluginLoader
 * FILE:        PluginLoadContext.cs
 * PURPOSE:     Basic Plugin Support, Load all Plugins
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

using System.Reflection;
using System.Runtime.Loader;

namespace PluginLoader
{
    /// <summary>
    /// Plugin Load Context.
    /// </summary>
    /// <seealso cref="System.Runtime.Loader.AssemblyLoadContext" />
    internal sealed class PluginLoadContext : AssemblyLoadContext
    {
        /// <summary>
        ///     The resolver
        /// </summary>
        private readonly AssemblyDependencyResolver _resolver;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:PluginLoader.PluginLoadContext" /> class.
        /// </summary>
        /// <param name="pluginPath">The plugin path.</param>
        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        /// <inheritdoc />
        /// <summary>
        ///     When overridden in a derived class, allows an assembly to be resolved and loaded based on its
        ///     <see cref="AssemblyName" />.
        /// </summary>
        /// <param name="assemblyName">The object that describes the assembly to be loaded.</param>
        /// <returns>
        ///     The loaded assembly, or <see langword="null" />.
        /// </returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Allows derived class to load an unmanaged library by name.
        /// </summary>
        /// <param name="unmanagedDllName">
        ///     Name of the unmanaged library. Typically this is the filename without its path or
        ///     extensions.
        /// </param>
        /// <returns>
        ///     A handle to the loaded library, or <see cref="nint.Zero" />.
        /// </returns>
        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : nint.Zero;
        }
    }
}
