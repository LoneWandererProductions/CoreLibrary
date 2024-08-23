using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin
{
    /// <summary>
    /// Abstract Implementation of Async Plugin
    /// The user can pick and choose what he needs in a cleaner way
    /// </summary>
    /// <seealso cref="Plugin.IAsyncPlugin" />
    public abstract class BaseAsyncPlugin : IAsyncPlugin
    {
        /// <summary>
        /// The event aggregator
        /// </summary>
        private IEventAggregator _eventAggregator;

        /// <summary>
        /// Gets or sets the event aggregator for the plugin.
        /// </summary>
        public IEventAggregator EventAggregator
        {
            get => _eventAggregator;
            set => _eventAggregator = value;
        }

        /// <summary>
        /// Gets the name.
        /// This field must be equal to the file name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name => "DefaultAsyncPlugin";

        /// <summary>
        /// Gets the type.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual string Type => "DefaultAsyncType";

        /// <summary>
        /// Gets the description.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public virtual string Description => "DefaultAsyncDescription";

        /// <summary>
        /// Gets the version.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual Version Version => new Version(1, 0);

        /// <summary>
        /// Gets the commands.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        public virtual List<Command> Commands => new List<Command>();

        /// <summary>
        /// Executes this instance asynchronously.
        /// </summary>
        /// <returns>Status Code as async</returns>
        public abstract Task<int> ExecuteAsync();

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Result object, async.</returns>
        public virtual Task<object> ExecuteCommandAsync(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the plugin type asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Status Code async</returns>
        public virtual Task<int> GetPluginTypeAsync(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <returns>Info about the plugin</returns>
        public virtual string GetInfo()
        {
            // Default implementation
            return "Default async plugin info";
        }

        /// <summary>
        /// Closes this instance asynchronously.
        /// </summary>
        /// <returns>Status Code</returns>
        public virtual Task<int> CloseAsync()
        {
            // Default implementation
            return Task.FromResult(0);
        }

        /// <summary>
        /// Publishes an event through the event aggregator.
        /// </summary>
        /// <param name="eventToPublish">The event to publish.</param>
        public void PublishEvent<TEvent>(TEvent eventToPublish)
        {
            _eventAggregator?.Publish(eventToPublish);
        }
    }
}
