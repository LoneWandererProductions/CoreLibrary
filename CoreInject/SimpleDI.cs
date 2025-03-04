using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreInject
{
    public class SimpleDI
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        // Register a Singleton
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            var instance = new TImplementation(); // Create once
            _registrations[typeof(TService)] = () => instance;
        }

        // Register a Transient
        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService, new()
        {
            _registrations[typeof(TService)] = () => new TImplementation(); // New instance every time
        }

        // Resolve Dependencies
        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }

        private object Resolve(Type serviceType)
        {
            if (_registrations.TryGetValue(serviceType, out var factory))
            {
                return factory();
            }

            // Try to create automatically (Constructor Injection)
            var constructor = serviceType.GetConstructors().FirstOrDefault();
            if (constructor == null)
                throw new Exception($"No constructor found for {serviceType.Name}");

            var parameters = constructor.GetParameters()
                .Select(param => Resolve(param.ParameterType))
                .ToArray();

            return Activator.CreateInstance(serviceType, parameters)
                   ?? throw new Exception($"Failed to create instance of {serviceType.Name}");
        }
    }
}
