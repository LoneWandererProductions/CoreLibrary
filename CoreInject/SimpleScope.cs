/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreWorker
 * FILE:        CoreInject/SimpleScope.cs
 * PURPOSE:     The Scope part of my Implementation, well mostly guess work.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;

namespace CoreInject
{
    /// <inheritdoc />
    /// <summary>
    ///     Manages a scope for resolving and tracking instances of services.
    ///     Ensures that services registered within the same scope return the same instance.
    /// </summary>
    public sealed class SimpleScope : IDisposable
    {
        /// <summary>
        ///     Stores instances of services that should persist within the current scope.
        /// </summary>
        private readonly Dictionary<Type, object> _scopedInstances = new();

        /// <summary>
        ///     Disposes all disposable objects registered in the scope and clears stored instances.
        /// </summary>
        public void Dispose()
        {
            foreach (var instance in _scopedInstances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _scopedInstances.Clear();
        }

        /// <summary>
        ///     Resolves a service within the scope, ensuring that the same instance is returned
        ///     for subsequent resolutions within the same scope.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">A factory function to create a new instance if none exists.</param>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided factory is null.</exception>
        public TService Resolve<TService>(Func<object> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory), "Factory function cannot be null.");
            }

            if (_scopedInstances.TryGetValue(typeof(TService), out var instance))
            {
                return (TService)instance;
            }

            instance = factory();
            _scopedInstances[typeof(TService)] = instance;
            return (TService)instance;
        }
    }
}
