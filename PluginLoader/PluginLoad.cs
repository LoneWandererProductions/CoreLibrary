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
        ///     Loads all.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Success Status</returns>
        public static bool LoadAll(string path)
        {
            var pluginPaths = GetFilesByExtensionFullPath(path);

            if (pluginPaths == null)
            {
                return false;
            }

            PluginContainer = new List<IPlugin>();

            foreach (var pluginPath in pluginPaths)
            {
                try
                {
                    var pluginAssembly = LoadPlugin(pluginPath);
                    var lst = CreateCommands(pluginAssembly).ToList();
                    PluginContainer.AddRange(lst);
                }
                catch (ArgumentException ex)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
                catch (FileLoadException ex)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
                catch (ApplicationException ex)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
                catch (BadImageFormatException ex)
                {
                    Trace.WriteLine(ex);
                    loadErrorEvent?.Invoke(nameof(LoadAll), new LoaderErrorEventArgs(ex.ToString()));
                }
                catch (FileNotFoundException ex)
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
        ///     / This method sets the these Variables.
        /// </param>
        /// <returns>Success Status</returns>
        public static bool SetEnvironmentVariables(Dictionary<int, object> store)
        {
            if (store == null)
            {
                return false;
            }

            //key, here we define the access able Environment for the plugins
            DataRegister.Store = store;

            return true;
        }

        /// <summary>
        ///     Gets the files by extension full path.
        ///     Adopted from FileHandler to decrease dependencies
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of files by extension with full path</returns>
        private static IEnumerable<string> GetFilesByExtensionFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Trace.WriteLine(PluginLoaderResources.ErrorEmptyPath);
                return null;
            }

            if (Directory.Exists(path))
            {
                return Directory.EnumerateFiles(path, PluginLoaderResources.FileExt,
                        SearchOption.TopDirectoryOnly)
                    .ToList();
            }

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
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        /// <summary>
        ///     Creates the commands.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Adds References to the Commands</returns>
        /// <exception cref="ApplicationException">
        ///     Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
        ///     $"Available types: {availableTypes}
        /// </exception>
        /// <exception cref="ArgumentException">Could not find the Plugin</exception>
        private static IEnumerable<IPlugin> CreateCommands(Assembly assembly)
        {
            var count = 0;

            foreach (var type in assembly.GetTypes().Where(type => typeof(IPlugin).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is not IPlugin result)
                {
                    continue;
                }

                count++;
                yield return result;
            }

            if (count != 0)
            {
                yield break;
            }

            var availableTypes =
                string.Join(PluginLoaderResources.Separator, assembly.GetTypes().Select(t => t.FullName));

            var message = string.Concat(PluginLoaderResources.ErrorCouldNotFindPlugin,
                PluginLoaderResources.Information(assembly, availableTypes));

            throw new ArgumentException(message);
        }
    }
}
