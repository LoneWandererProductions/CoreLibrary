/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/BaseAsyncPlugin.cs
 * PURPOSE:     Basic abstract Plugin Implementation, in this case for async
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 *              https://medium.com/c-sharp-progarmming/wpf-application-with-plugin-architecture-30004f3319d3
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin
{
    /// <inheritdoc />
    /// <summary>
    ///     Abstract Implementation of Async Plugin
    ///     The user can pick and choose what he needs in a cleaner way
    /// </summary>
    /// <seealso cref="T:Plugin.IAsyncPlugin" />
    public abstract class BaseAsyncPlugin : IAsyncPlugin
    {
        /// <inheritdoc />
        /// <summary>
        ///     Gets or sets the event aggregator for the plugin.
        /// </summary>
        public IEventAggregator EventAggregator { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the name.
        ///     This field must be equal to the file name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public virtual string Name => "DefaultAsyncPlugin";

        /// <inheritdoc />
        /// <summary>
        ///     Gets the type.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public virtual string Type => "DefaultAsyncType";

        /// <inheritdoc />
        /// <summary>
        ///     Gets the description.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public virtual string Description => "DefaultAsyncDescription";

        /// <inheritdoc />
        /// <summary>
        ///     Gets the version.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public virtual Version Version => new(1, 0);

        /// <inheritdoc />
        /// <summary>
        ///     Gets the commands.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The commands.
        /// </value>
        public virtual List<Command> Commands => new();

        /// <inheritdoc />
        /// <summary>
        ///     Executes this instance asynchronously.
        /// </summary>
        /// <returns>Status Code as async</returns>
        public abstract Task<int> ExecuteAsync();

        /// <inheritdoc />
        /// <summary>
        ///     Executes the command asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Result object, async.</returns>
        public virtual Task<object> ExecuteCommandAsync(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the plugin type asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Status Code async</returns>
        public virtual Task<int> GetPluginTypeAsync(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the information.
        /// </summary>
        /// <returns>Info about the plugin</returns>
        public virtual string GetInfo()
        {
            // Default implementation
            return "Default async plugin info";
        }

        /// <inheritdoc />
        /// <summary>
        ///     Closes this instance asynchronously.
        /// </summary>
        /// <returns>Status Code</returns>
        public virtual Task<int> CloseAsync()
        {
            // Default implementation
            return Task.FromResult(0);
        }

        /// <inheritdoc cref="Action" />
        /// <summary>
        ///     Publishes an event through the event aggregator.
        /// </summary>
        /// <param name="eventToPublish">The event to publish.</param>
        public void PublishEvent<TEvent>(TEvent eventToPublish)
        {
            EventAggregator?.Publish(eventToPublish);
        }
    }
}
