/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        ImageDecoderPluginRegistry.cs
 * PURPOSE:     Discovers, loads, and looks up IImageDecoderPlugin implementations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Imaging.Interfaces;

namespace Imaging
{
    /// <summary>
    ///     Finds decoder plugins in a folder, loads them, and answers "does anyone
    ///     handle this extension?" for <c>ImageStream.GetOriginalBitmap</c>.
    /// </summary>
    /// <remarks>
    ///     Deliberately simple: <see cref="Assembly.LoadFrom(string)" /> plus
    ///     reflection, not a full plugin framework (MEF, AssemblyLoadContext
    ///     isolation, hot-unload). SlimViewer's plugins are trusted, first-party,
    ///     restart-to-update DLLs, not untrusted or hot-swappable code, so the
    ///     extra machinery a general-purpose plugin host needs isn't buying
    ///     anything here. If that ever changes - loading plugins you didn't write,
    ///     or needing to unload/reload one without restarting - an
    ///     AssemblyLoadContext-per-plugin is the natural next step; the
    ///     <see cref="IImageDecoderPlugin" /> contract doesn't need to change to
    ///     get there.
    /// </remarks>
    public sealed class ImageDecoderPluginRegistry
    {
        private static readonly Lazy<ImageDecoderPluginRegistry> LazyInstance =
            new(() => new ImageDecoderPluginRegistry());

        private readonly Dictionary<string, IImageDecoderPlugin> _byExtension =
            new(StringComparer.OrdinalIgnoreCase);

        private ImageDecoderPluginRegistry()
        {
        }

        /// <summary>
        ///     Gets the singleton instance.
        /// </summary>
        public static ImageDecoderPluginRegistry Instance => LazyInstance.Value;

        /// <summary>
        ///     Gets the plugins currently loaded, for a diagnostics/about screen if
        ///     one is ever wanted.
        /// </summary>
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

            foreach (var ext in plugin.SupportedExtensions)
            {
                var normalized = NormalizeExtension(ext);
                _byExtension[normalized] = plugin;

                if (!ImagingResources.Appendix.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    ImagingResources.Appendix.Add(normalized);
                }
            }

            Trace.WriteLine(
                $"[ImageDecoderPluginRegistry] Loaded '{plugin.Name}' for {string.Join(", ", plugin.SupportedExtensions)}");
        }

        /// <summary>
        ///     Looks up whether a plugin handles <paramref name="extension" />.
        /// </summary>
        /// <param name="extension">Extension including the leading dot, any case.</param>
        public bool TryGetDecoder(string? extension, out IImageDecoderPlugin? plugin)
        {
            plugin = null;
            return !string.IsNullOrEmpty(extension) &&
                   _byExtension.TryGetValue(NormalizeExtension(extension), out plugin);
        }

        private void LoadPluginsFromAssembly(string dllPath)
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(dllPath);
            }
            catch (Exception ex) when (ex is BadImageFormatException or FileLoadException or IOException)
            {
                Trace.WriteLine($"[ImageDecoderPluginRegistry] Could not load '{dllPath}': {ex}");
                return;
            }

            IEnumerable<Type> candidateTypes;
            try
            {
                candidateTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Some types in the assembly failed to load (e.g. a dependency
                // this plugin needs isn't present) - use whichever types DID
                // load rather than discarding the whole assembly over it.
                candidateTypes = ex.Types.OfType<Type>();
                Trace.WriteLine($"[ImageDecoderPluginRegistry] Partial load for '{dllPath}': {ex}");
            }

            foreach (var type in candidateTypes)
            {
                if (type is not { IsClass: true, IsAbstract: false } ||
                    !typeof(IImageDecoderPlugin).IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    if (Activator.CreateInstance(type) is IImageDecoderPlugin plugin)
                    {
                        Register(plugin);
                    }
                }
                catch (Exception ex) when (ex is MissingMethodException or TargetInvocationException)
                {
                    // A plugin whose constructor throws (bad config, missing
                    // resource, etc.) is skipped, not fatal to the others.
                    Trace.WriteLine($"[ImageDecoderPluginRegistry] Failed to construct '{type.FullName}': {ex}");
                }
            }
        }

        private static string NormalizeExtension(string extension)
        {
            var trimmed = extension.Trim();
            return trimmed.StartsWith('.') ? trimmed.ToLowerInvariant() : "." + trimmed.ToLowerInvariant();
        }
    }
}
