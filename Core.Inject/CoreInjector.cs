/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Inject
 * FILE:        CoreInjector.cs
 * PURPOSE:     Manages service registrations and resolutions for dependency injection
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Inject
{
    /// <summary>
    ///     A simple dependency injection container that handles the registration and resolution of services.
    ///     Supports singleton, transient, scoped, and instance-based registrations.
    /// </summary>
    public sealed class CoreInjector : IDisposable
    {
        /// <summary>
        ///     Stores the registered service factories, indexed by service type.
        /// </summary>
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        /// <summary>
        /// The singleton instances
        /// </summary>
        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new();

        /// <summary>
        ///     The current scope for managing scoped dependencies.
        /// </summary>
        private readonly AsyncLocal<SimpleScope?> _currentScope = new();

        /// <summary>
        ///     Registers a singleton service with a specific implementation type.
        ///     The same instance of the service is returned every time it is resolved.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown if the service is already registered.</exception>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            _registrations[typeof(TService)] = () =>
                _singletonInstances.GetOrAdd(typeof(TService), _ => CreateInstance(typeof(TImplementation)));
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
                throw new InvalidOperationException(string.Format(CoreInjectResource.ErrorServiceRegistered,
                    typeof(TService).Name));
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
                if (_currentScope.Value == null)
                {
                    throw new InvalidOperationException(CoreInjectResource.ErrorNoActiveScope);
                }

                return _currentScope.Value.Resolve<TService>(() => new TImplementation());
            };
        }

        /// <summary>
        /// Registers the transient.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <exception cref="System.InvalidOperationException">Service {serviceType.Name} is already registered.</exception>
        public void RegisterTransient(Type serviceType, Type implementationType)
        {
            if (_registrations.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Service {serviceType.Name} is already registered.");
            }

            // We store a factory that calls our new CreateInstance method
            _registrations[serviceType] = () => CreateInstance(implementationType);
        }

        /// <summary>
        ///     Resolves an instance of the requested service type.
        ///     If a scope is active, the service will be resolved within that scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public TService Resolve<TService>()
        {
            if (_currentScope.Value != null && _registrations.ContainsKey(typeof(TService)))
            {
                return _currentScope.Value.Resolve<TService>(() => (TService)_registrations[typeof(TService)]());
            }

            return (TService)Resolve(typeof(TService));
        }

        /// <summary>
        ///     Starts a new dependency scope.
        ///     Scoped services can be resolved within this scope.
        /// </summary>
        public void BeginScope()
        {
            _currentScope.Value = new SimpleScope();
        }

        /// <summary>
        ///     Ends the current dependency scope.
        ///     Scoped services will no longer be available after this point.
        /// </summary>
        public void EndScope()
        {
            _currentScope.Value?.Dispose();
            _currentScope.Value = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Only dispose things we actually created!
            foreach (var instance in _singletonInstances.Values)
            {
                (instance as IDisposable)?.Dispose();
            }

            _singletonInstances.Clear();
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
                throw new InvalidOperationException(string.Format(CoreInjectResource.ErrorServiceNotRegistered,
                    serviceType.Name));
            }

            return factory();
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <returns>The created instance.</returns>
        private object CreateInstance(Type implementationType)
        {
            // Find the best constructor (for now, we'll just take the first one)
            var constructor = implementationType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length) // Pick the one with the most params
                .First();

            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(implementationType);
            }

            // Recursively resolve each dependency found in the constructor
            var parameterInstances = new List<object>();
            foreach (var parameter in parameters)
            {
                parameterInstances.Add(Resolve(parameter.ParameterType));
            }

            return constructor.Invoke(parameterInstances.ToArray());
        }
    }
}
