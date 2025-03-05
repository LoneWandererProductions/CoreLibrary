using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreInject
{
    public class SimpleScope
    {
        private readonly Dictionary<Type, object> _scopedInstances = new();

        public TService Resolve<TService>(Func<object> factory)
        {
            if (!_scopedInstances.TryGetValue(typeof(TService), out var instance))
            {
                instance = factory();
                _scopedInstances[typeof(TService)] = instance;
            }
            return (TService)instance;
        }

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
    }

}
