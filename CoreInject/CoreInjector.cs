/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreWorker
 * FILE:        CoreInject/CoreInjector.cs
 * PURPOSE:     Manages service registrations and resolutions for dependency injection
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace CoreInject
{
    /// <summary>
    ///     A simple dependency injection container that handles the registration and resolution of services.
    ///     Supports singleton, transient, scoped, and instance-based registrations.
    /// </summary>
    public sealed class CoreInjector
    {
        /// <summary>
        ///     Stores the registered service factories, indexed by service type.
        /// </summary>
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        /// <summary>
        ///     The current scope for managing scoped dependencies.
        /// </summary>
        private SimpleScope? _currentScope;

        /// <summary>
        ///     Registers a singleton service with a specific implementation type.
        ///     The same instance of the service is returned every time it is resolved.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered.</exception>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException(
                    $"Service {typeof(TService).Name} is already registered as singleton.");
            }

            TImplementation instance = new();
            _registrations[typeof(TService)] = () => instance; // Always return the same instance
        }

        /// <summary>
        ///     Registers a singleton service with a factory function.
        ///     The factory function is used to create the service when it is resolved.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">A factory function that creates the service, accepting the CoreInjector instance.</param>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered as a singleton.</exception>
        public void RegisterSingleton<TService>(Func<CoreInjector, TService> factory)
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException(
                    $"Service {typeof(TService).Name} is already registered as singleton.");
            }

            _registrations[typeof(TService)] = () => factory(this); // Pass 'this' to factory
        }

        /// <summary>
        ///     Registers an existing instance as a service.
        ///     The same instance is returned every time the service is resolved.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The instance to register as the service.</param>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered.</exception>
        public void RegisterInstance<TService>(TService instance)
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service {typeof(TService).Name} is already registered.");
            }

            _registrations[typeof(TService)] = () => instance;
        }

        /// <summary>
        ///     Registers a transient service, meaning a new instance is created each time it is resolved.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered.</exception>
        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service {typeof(TService).Name} is already registered.");
            }

            _registrations[typeof(TService)] = () => new TImplementation();
        }

        /// <summary>
        ///     Registers a scoped service, meaning the same instance is used within a scope.
        ///     A scope must be active for this service to be resolved.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if there is no active scope.</exception>
        public void RegisterScoped<TService, TImplementation>() where TImplementation : TService, new()
        {
            _registrations[typeof(TService)] = () =>
            {
                if (_currentScope == null)
                {
                    throw new InvalidOperationException("No active scope. Call BeginScope() first.");
                }

                return _currentScope.Resolve<TService>(() => new TImplementation());
            };
        }

        /// <summary>
        ///     Resolves an instance of the requested service type.
        ///     If a scope is active, the service will be resolved within that scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public TService Resolve<TService>()
        {
            if (_currentScope != null && _registrations.ContainsKey(typeof(TService)))
            {
                return _currentScope.Resolve<TService>(() => (TService)_registrations[typeof(TService)]());
            }

            return (TService)Resolve(typeof(TService));
        }

        /// <summary>
        ///     Resolves a service by its type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service cannot be resolved.</exception>
        private object Resolve(Type serviceType)
        {
            if (!_registrations.TryGetValue(serviceType, out var factory))
            {
                throw new InvalidOperationException($"Service {serviceType.Name} is not registered.");
            }

            return factory();
        }

        /// <summary>
        ///     Starts a new dependency scope.
        ///     Scoped services can be resolved within this scope.
        /// </summary>
        public void BeginScope()
        {
            _currentScope = new SimpleScope();
        }

        /// <summary>
        ///     Ends the current dependency scope.
        ///     Scoped services will no longer be available after this point.
        /// </summary>
        public void EndScope()
        {
            _currentScope?.Dispose();
            _currentScope = null;
        }
    }
}
