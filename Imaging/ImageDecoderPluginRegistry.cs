/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        ImageDecoderPluginRegistry.cs
 * PURPOSE:     Discovers, loads, and looks up IImageDecoderPlugin implementations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Helpers;
using Imaging.Plugins.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Imaging
{
    /// <summary>
    ///     Finds decoder plugins in a folder, loads them, and answers "does anyone
    ///     handle this extension?" for <c>ImageStream.GetOriginalBitmap</c>.
    /// </summary>
    /// <remarks>
    ///     Deliberately simple: plugins are trusted, first-party DLLs and are
    ///     loaded with an AssemblyLoadContext and AssemblyDependencyResolver so
    ///     that plugin-specific managed and native dependencies remain next to
    ///     the plugin. There is no hot-unload or isolation requirement.
    /// </remarks>
    public sealed class ImageDecoderPluginRegistry
    {
        /// <summary>
        /// The lazy instance
        /// </summary>
        private static readonly Lazy<ImageDecoderPluginRegistry> LazyInstance =
            new(() => new ImageDecoderPluginRegistry());

        /// <summary>
        /// The by extension
        /// </summary>
        private readonly Dictionary<string, IImageDecoderPlugin> _byExtension =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The plugins
        /// </summary>
        private readonly List<IImageDecoderPlugin> _plugins = new();

        /// <summary>
        /// The load contexts
        /// </summary>
        private readonly List<PluginLoadContext> _loadContexts = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="ImageDecoderPluginRegistry"/> class from being created.
        /// </summary>
        private ImageDecoderPluginRegistry()
        {
        }

        /// <summary>
        ///     Gets the singleton instance.
        /// </summary>
        public static ImageDecoderPluginRegistry Instance => LazyInstance.Value;

        /// <summary>
        /// Gets the plugins currently loaded, for a diagnostics/about screen if
        /// one is ever wanted.
        /// </summary>
        /// <value>
        /// The loaded plugins.
        /// </value>
        public IReadOnlyCollection<IImageDecoderPlugin> LoadedPlugins => _byExtension.Values.Distinct().ToList();

        /// <summary>
        ///     Scans <paramref name="pluginDirectory" /> for DLLs containing
        ///     <see cref="IImageDecoderPlugin" /> implementations, instantiates
        ///     each one found, and merges its extensions into both this registry
        ///     and <see cref="ImagingResources.Appendix" />.
        /// </summary>
        /// <param name="pluginDirectory">
        ///     Folder to scan. Safe to call with a folder that doesn't exist yet
        ///     (e.g. a fresh install with no plugins dropped in) - this is a no-op
        ///     in that case, not an error.
        /// </param>
        /// <remarks>
        ///     One bad DLL - wrong .NET version, missing dependency, a type whose
        ///     constructor throws - is logged and skipped. It never stops the
        ///     other plugins in the folder from loading, and it never stops the
        ///     app from starting.
        /// </remarks>
        public void LoadFromDirectory(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
            {
                return;
            }

            foreach (var dllPath in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                LoadPluginsFromAssembly(dllPath);
            }
        }

        /// <summary>
        ///     Registers a plugin instance directly, bypassing assembly scanning.
        ///     Useful for unit tests, or for a plugin the host app wants to wire
        ///     up explicitly instead of dropping in the Plugins folder.
        /// </summary>
        public void Register(IImageDecoderPlugin plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            _plugins.Add(plugin);

            // We still register extensions for the UI/Appendix, but NOT for routing
            foreach (var ext in plugin.SupportedExtensions)
            {
                var normalized = NormalizeExtension(ext);
                if (!ImagingResources.Appendix.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    ImagingResources.Appendix.Add(normalized);
                }
            }

            Trace.WriteLine($"[ImageDecoderPluginRegistry] Loaded '{plugin.Name}'");
        }

        /// <summary>
        /// Looks up whether a plugin handles <paramref name="extension" />.
        /// </summary>
        /// <param name="headerBytes">The header bytes.</param>
        /// <param name="plugin">The plugin.</param>
        /// <returns><c>true</c> if a plugin was found; otherwise, <c>false</c>.</returns>
        public bool TryGetDecoder(byte[] headerBytes, out IImageDecoderPlugin? plugin)
        {
            // Iterate through plugins and let them inspect the magic numbers
            plugin = _plugins.FirstOrDefault(p => p.CanDecode(headerBytes));
            return plugin != null;
        }

        /// <summary>
        /// Loads the plugins from assembly.
        /// </summary>
        /// <param name="dllPath">The DLL path.</param>
        private void LoadPluginsFromAssembly(string dllPath)
        {
            Assembly assembly;

            try
            {
                var fullPath = Path.GetFullPath(dllPath);

                var loadContext = new PluginLoadContext(fullPath);
                _loadContexts.Add(loadContext);

                assembly = loadContext.LoadFromAssemblyPath(fullPath);
            }
            catch (Exception ex) when (
                ex is BadImageFormatException or
                FileLoadException or
                IOException)
            {
                Trace.WriteLine(
                    $"[ImageDecoderPluginRegistry] Could not load '{dllPath}': {ex}");

                return;
            }

            IEnumerable<Type> candidateTypes;

            try
            {
                candidateTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                candidateTypes = ex.Types.OfType<Type>();

                Trace.WriteLine(
                    $"[ImageDecoderPluginRegistry] Partial load for '{dllPath}': {ex}");
            }

            foreach (var type in candidateTypes)
            {
                if (type is not
                    {
                        IsClass: true,
                        IsAbstract: false
                    } ||
                    !typeof(IImageDecoderPlugin).IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    if (Activator.CreateInstance(type)
                        is IImageDecoderPlugin plugin)
                    {
                        Register(plugin);
                    }
                }
                catch (Exception ex) when (
                    ex is MissingMethodException or
                    TargetInvocationException)
                {
                    Trace.WriteLine(
                        $"[ImageDecoderPluginRegistry] " +
                        $"Failed to construct '{type.FullName}': {ex}");
                }
            }
        }

        /// <summary>
        /// Normalizes the extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>The normalized extension.</returns>
        private static string NormalizeExtension(string extension)
        {
            var trimmed = extension.Trim();
            return trimmed.StartsWith('.') ? trimmed.ToLowerInvariant() : "." + trimmed.ToLowerInvariant();
        }
    }
}
