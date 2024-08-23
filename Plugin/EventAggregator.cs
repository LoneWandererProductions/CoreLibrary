using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Plugin
{

    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object>>> _subscribers = new();

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            if (_subscribers.TryGetValue(typeof(TEvent), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        // Safely cast and invoke the handler
                        var typedHandler = handler as Action<TEvent>;
                        typedHandler?.Invoke(eventToPublish);
                    }
                    catch (Exception ex)
                    {
                        // Handle or log exception
                        Console.WriteLine($"Error in event handler: {ex}");
                    }
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> eventHandler)
        {
            _subscribers.AddOrUpdate(
                typeof(TEvent),
                _ => new ConcurrentBag<Action<object>> { e => eventHandler((TEvent)e) },
                (_, handlers) =>
                {
                    handlers.Add(e => eventHandler((TEvent)e));
                    return handlers;
                });
        }

        public void Unsubscribe<TEvent>(Action<TEvent> eventHandler)
        {
            if (_subscribers.TryGetValue(typeof(TEvent), out var handlers))
            {
                // Create a new list of handlers excluding the handler to be removed
                var newHandlers = handlers.Where(h => !IsSameHandler(h, eventHandler)).ToList();

                // Replace the old list with the new one
                _subscribers[typeof(TEvent)] = new ConcurrentBag<Action<object>>(newHandlers);
            }
        }

        private bool IsSameHandler<TEvent>(Action<object> storedHandler, Action<TEvent> handlerToRemove)
        {
            return storedHandler.Target == handlerToRemove.Target && storedHandler.Method == handlerToRemove.Method;
        }
    }
}
