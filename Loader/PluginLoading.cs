/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Loader
 * FILE:        PluginLoading.cs
 * PURPOSE:     Generic plugin loader that loads implementations of a given contract.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Reflection;

namespace Loader
{
    /// <summary>
    /// Generic plugin loader that loads implementations of a given contract.
    /// </summary>
    public sealed class PluginLoading
    {
        /// <summary>
        /// Loads all implementations of <typeparamref name="TContract"/> from the specified directory.
        /// </summary>
        /// <typeparam name="TContract">The contract interface or base class.</typeparam>
        /// <param name="directory">Directory containing plugin assemblies.</param>
        /// <returns>Loaded plugin instances.</returns>
        public IReadOnlyList<TContract> Load<TContract>(string directory)
            where TContract : class
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var result = new List<TContract>();

            // Safety check: if the directory isn't there, there are no plugins to load.
            if (!Directory.Exists(directory))
                return result;

            foreach (var file in Directory.EnumerateFiles(directory, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(Path.GetFullPath(file));

                    foreach (var type in asm.GetTypes())
                    {
                        if (type.IsAbstract || type.IsInterface)
                            continue;

                        if (typeof(TContract).IsAssignableFrom(type))
                        {
                            if (Activator.CreateInstance(type) is TContract instance)
                                result.Add(instance);
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    // Skip files that aren't valid .NET assemblies (e.g., native DLLs)
                    continue;
                }
            }

            return result;
        }
    }
}
