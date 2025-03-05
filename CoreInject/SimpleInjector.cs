using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreInject
{
    public class SimpleInjector
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        private Dictionary<Type, object> _scopedInstances = new();

        private SimpleScope? _currentScope;

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new Exception($"Service {typeof(TService).Name} is already registered.");
            }


            var instance = new TImplementation(); // Create once
            _registrations[typeof(TService)] = () => instance;
        }

        public void RegisterInstance<TService>(TService instance)
        {
            _registrations[typeof(TService)] = () => instance;
        }

        /// <summary>
        /// Registers the transient.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService, new()
        {
            if (_registrations.ContainsKey(typeof(TService)))
            {
                throw new Exception($"Service {typeof(TService).Name} is already registered.");
            }

            _registrations[typeof(TService)] = () => new TImplementation(); // New instance every time
        }

        /// <summary>
        /// Registers the scoped.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
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
        /// Resolves this instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns></returns>
        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }

        /// <summary>
        /// Resolves the specified service type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// No constructor found for {serviceType.Name}
        /// or
        /// Failed to create instance of {serviceType.Name}
        /// </exception>
        private object Resolve(Type serviceType)
        {
            // If the service type is an interface or abstract class, we need to check if it's registered
            if (!_registrations.ContainsKey(serviceType) && (serviceType.IsInterface || serviceType.IsAbstract))
            {
                throw new Exception($"Service {serviceType.Name} is not registered.");
            }

            // If the service is registered, resolve it
            if (_registrations.ContainsKey(serviceType))
            {
                return _registrations[serviceType]();
            }

            // Try to create automatically (Constructor Injection)
            var constructor = serviceType.GetConstructors().FirstOrDefault()
                              ?? throw new Exception($"No constructor found for {serviceType.Name}");

            var parameters = constructor.GetParameters()
                .Select(param => Resolve(param.ParameterType))
                .ToArray();

            return Activator.CreateInstance(serviceType, parameters)
                   ?? throw new Exception($"Failed to create instance of {serviceType.Name}");
        }

        public void BeginScope()
        {
            _currentScope = new SimpleScope();
        }

        public void EndScope()
        {
            _currentScope?.Dispose();
            _currentScope = null;
        }
    }
}
