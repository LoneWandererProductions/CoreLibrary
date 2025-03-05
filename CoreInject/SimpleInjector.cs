using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreInject
{
    public sealed class SimpleInjector
    {
        /// <summary>
        /// Stores registered service factories.
        /// </summary>
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        /// <summary>
        /// The current scope for scoped dependencies.
        /// </summary>
        private SimpleScope? _currentScope;

        /// <summary>
        /// Registers a singleton instance of a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service {typeof(TService).Name} is already registered as singleton.");
            }

            TImplementation instance = new();
            _registrations[typeof(TService)] = () => instance; // Always return the same instance
        }


        /// <summary>
        /// Registers a pre-existing instance as a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public void RegisterInstance<TService>(TService instance)
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service {typeof(TService).Name} is already registered.");
            }

            _registrations[typeof(TService)] = () => instance;
        }

        /// <summary>
        /// Registers a service as transient, meaning a new instance is created each time.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new InvalidOperationException($"Service {typeof(TService).Name} is already registered.");
            }

            _registrations[typeof(TService)] = () => new TImplementation();
        }

        /// <summary>
        /// Registers a service as scoped, meaning the instance persists within a scope.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
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
        /// Resolves an instance of the requested service type.
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
        /// Resolves a service by its type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The resolved service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved.</exception>
        private object Resolve(Type serviceType)
        {
            if (!_registrations.TryGetValue(serviceType, out var factory))
            {
                throw new InvalidOperationException($"Service {serviceType.Name} is not registered.");
            }

            return factory();
        }

        /// <summary>
        /// Starts a new dependency scope.
        /// </summary>
        public void BeginScope()
        {
            _currentScope = new SimpleScope();
        }

        /// <summary>
        /// Ends the current dependency scope.
        /// </summary>
        public void EndScope()
        {
            _currentScope?.Dispose();
            _currentScope = null;
        }
    }
}
