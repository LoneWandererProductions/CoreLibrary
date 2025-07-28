/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/IEventAggregator.cs
 * PURPOSE:     Event Interface EventAggregator
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace Plugin;

/// <summary>
///     Interface for Event Aggregator
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    ///     Publishes the specified event to publish.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="eventToPublish">The event to publish.</param>
    void Publish<TEvent>(TEvent eventToPublish);

    /// <summary>
    ///     Subscribes the specified event handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="eventHandler">The event handler.</param>
    void Subscribe<TEvent>(Action<TEvent> eventHandler);

    /// <summary>
    ///     Unsubscribes the specified event handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="eventHandler">The event handler.</param>
    void Unsubscribe<TEvent>(Action<TEvent> eventHandler);
}
