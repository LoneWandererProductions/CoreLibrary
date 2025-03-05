﻿
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreInject
{
    public static class SimpleInjectorExtensions
    {
        /// <summary>
        /// Automatically registers all classes that implement an interface in the given assembly.
        /// </summary>
        /// <param name="injector">The SimpleInjector instance.</param>
        /// <param name="assembly">The assembly to scan for implementations.</param>
        public static void RegisterFromAssembly(this SimpleInjector injector, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i });

            foreach (var type in types.Where(type => !injector.IsServiceRegistered(type.Interface)))
            {
                // Try to register the service
                try
                {
                    var method = typeof(SimpleInjector)
                        .GetMethod("RegisterTransient", BindingFlags.Public | BindingFlags.Instance)
                        ?.MakeGenericMethod(type.Interface, type.Implementation);

                    method?.Invoke(injector, null);
                }
                catch (InvalidOperationException ex)
                {
                    // Log and handle any errors if needed, this should not happen anymore
                    throw new InvalidOperationException($"Error registering {type.Interface.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Automatically registers all classes that implement an interface in the given assembly.
        /// </summary>
        /// <param name="injector">The SimpleInjector instance.</param>
        /// <param name="assemblies">The assemblies to scan for implementations.</param>
        public static void RegisterFromAssemblies(this SimpleInjector injector, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i });

                foreach (var type in types.Where(type => !injector.IsServiceRegistered(type.Interface)))
                {
                    // Try to register the service
                    try
                    {
                        var method = typeof(SimpleInjector)
                            .GetMethod("RegisterTransient", BindingFlags.Public | BindingFlags.Instance)
                            ?.MakeGenericMethod(type.Interface, type.Implementation);

                        method?.Invoke(injector, null);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Log and handle any errors if needed, this should not happen anymore
                        throw new InvalidOperationException($"Error registering {type.Interface.Name}: {ex.Message}");
                    }
                }
            }
        }


        private static bool IsServiceRegistered(this SimpleInjector injector, Type serviceType)
        {
            // Check if the service is already registered in the injector
            var field = typeof(SimpleInjector)
                .GetField("_registrations", BindingFlags.NonPublic | BindingFlags.Instance);
            var registrations = (Dictionary<Type, Func<object>>)field.GetValue(injector);
            return registrations.ContainsKey(serviceType);
        }

        /// <summary>
        /// Attempts to resolve a service without throwing an exception if not found.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="injector">The SimpleInjector instance.</param>
        /// <returns>The resolved service or null if not found.</returns>
        public static TService? TryResolve<TService>(this SimpleInjector injector) where TService : class
        {
            try
            {
                return injector.Resolve<TService>();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
