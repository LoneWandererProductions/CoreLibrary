/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        PluginLoader/PluginLoader.cs
 * PURPOSE:     Basic Plugin Support, Load all Plugins
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Plugin;

namespace PluginLoader
{
    /// <summary>
    ///     Basic Load System for the Plugins
    /// </summary>
    public static class PluginLoad
    {
        /// <summary>
        ///     The load error event
        /// </summary>
        public static EventHandler loadErrorEvent;

        /// <summary>
        ///     Gets or sets the plugin container.
        /// </summary>
        /// <value>
        ///     The plugin container.
        /// </value>
        public static List<IPlugin> PluginContainer { get; private set; }

        /// <summary>
        /// Loads all.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extension">The extension for plugins.</param>
        /// <returns>
        /// Success Status
        /// </returns>
        public static bool LoadAll(string path, string extension = PluginLoaderResources.FileExt)
        {
            var pluginPaths = GetFilesByExtensionFullPath(path, extension);

            if (pluginPaths == null) return false;

            PluginContainer = new List<IPlugin>();

            foreach (var pluginPath in pluginPaths)
            {
                try
                {
                    var pluginAssembly = LoadPlugin(pluginPath);
                    var lst = CreateCommands(pluginAssembly).ToList();
                    PluginContainer.AddRange(lst);
                }
                catch (Exception ex) when (ex is ArgumentException ||
                                           ex is FileLoadException ||
                                           ex is ApplicationException ||
                                           ex is ReflectionTypeLoadException ||
                                           ex is BadImageFormatException ||
                                           ex is FileNotFoundException)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
            }

            return PluginContainer.Count != 0;
        }

        /// <summary>
        ///     Loads all.
        /// </summary>
        /// <param name="store">
        ///     Sets the environment variables of the base module
        ///     The idea is, the main module has documented Environment Variables, that the plugins can use.
        ///     / This method sets these Variables.
        /// </param>
        /// <returns>Success Status</returns>
        public static bool SetEnvironmentVariables(Dictionary<int, object> store)
        {
            if (store == null) return false;

            // Key, here we define the accessible Environment for the plugins
            DataRegister.Store = store;

            return true;
        }

        /// <summary>
        ///     Gets the files by extension full path.
        ///     Adopted from FileHandler to decrease dependencies
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extension">The custom file extension.</param>
        /// <returns>List of files by extension with full path</returns>
        private static IEnumerable<string> GetFilesByExtensionFullPath(string path, string extension)
        {
            if (string.IsNullOrEmpty(path))
            {
                Trace.WriteLine(PluginLoaderResources.ErrorPath);
                return null;
            }

            if (Directory.Exists(path))
                return Directory.EnumerateFiles(path, $"*{extension}", SearchOption.TopDirectoryOnly).ToList();

            Trace.WriteLine(PluginLoaderResources.ErrorDirectory);

            return null;
        }

        /// <summary>
        ///     Loads the plugin.
        /// </summary>
        /// <param name="pluginLocation">The plugin location.</param>
        /// <returns>An Assembly</returns>
        private static Assembly LoadPlugin(string pluginLocation)
        {
            var loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyPath(pluginLocation);
        }

        /// <summary>
        ///     Creates the commands.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Adds References to the Commands</returns>
        /// <exception cref="ApplicationException">
        ///     Can't find any type which implements IPlugin in {assembly} from {assembly.Location}.\n" +
        ///     $"Available types: {availableTypes}
        /// </exception>
        /// <exception cref="ArgumentException">Could not find the Plugin</exception>
        private static IEnumerable<IPlugin> CreateCommands(Assembly assembly)
        {
            var count = 0;

            foreach (var type in assembly.GetTypes().Where(type => typeof(IPlugin).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is not IPlugin result) continue;

                count++;
                yield return result;
            }

            if (count != 0) yield break;

            var availableTypes = string.Join(PluginLoaderResources.Separator, assembly.GetTypes().Select(t => t.FullName));

            var message = string.Concat(PluginLoaderResources.ErrorCouldNotFindPlugin,
                PluginLoaderResources.Information(assembly, availableTypes));

            throw new ArgumentException(message);
        }
    }
}
