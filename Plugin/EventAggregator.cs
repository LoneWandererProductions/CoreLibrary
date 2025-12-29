/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/EventAggregator.cs
 * PURPOSE:     Event EventAggregator
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Plugin
{
    /// <inheritdoc />
    /// <summary>
    ///     Collects all events of the plugins
    /// </summary>
    /// <seealso cref="IEventAggregator" />
    public class EventAggregator : IEventAggregator
    {
        /// <summary>
        ///     The subscribers
        /// </summary>
        private readonly ConcurrentDictionary<Type, ConcurrentBag<Action<object>>> _subscribers = new();

        /// <inheritdoc />
        /// <summary>
        ///     Publishes the specified event to publish.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="eventToPublish">The event to publish.</param>
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

        /// <inheritdoc />
        /// <summary>
        ///     Subscribes the specified event handler.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
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

        /// <inheritdoc />
        /// <summary>
        ///     Unsubscribes the specified event handler.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> eventHandler)
        {
            if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers))
            {
                return;
            }

            // Create a new list of handlers excluding the handler to be removed
            var newHandlers = handlers.Where(h => !IsSameHandler(h, eventHandler)).ToList();

            // Replace the old list with the new one
            _subscribers[typeof(TEvent)] = new ConcurrentBag<Action<object>>(newHandlers);
        }

        /// <inheritdoc cref="Action" />
        /// <summary>
        ///     Determines whether [is same handler] [the specified stored handler].
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="storedHandler">The stored handler.</param>
        /// <param name="handlerToRemove">The handler to remove.</param>
        /// <returns>
        ///     <c>true</c> if [is same handler] [the specified stored handler]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSameHandler<TEvent>(Action<object> storedHandler, Action<TEvent> handlerToRemove)
        {
            return storedHandler.Target == handlerToRemove.Target && storedHandler.Method == handlerToRemove.Method;
        }
    }
}
