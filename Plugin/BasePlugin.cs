/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/BasePlugin.cs
 * PURPOSE:     Basic abstract Plugin Implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 *              https://medium.com/c-sharp-progarmming/wpf-application-with-plugin-architecture-30004f3319d3
 */

using System;
using System.Collections.Generic;

namespace Plugin
{
    /// <inheritdoc />
    /// <summary>
    ///     Abstract Implementation of Plugin
    ///     The user can pick and choose what he needs in a cleaner ways
    /// </summary>
    /// <seealso cref="Plugin.IPlugin" />
    public abstract class BasePlugin : IPlugin
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
        public virtual string Name => "DefaultPlugin";

        /// <inheritdoc />
        /// <summary>
        ///     Gets the type.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public virtual string Type => "DefaultType";

        /// <inheritdoc />
        /// <summary>
        ///     Gets the description.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public virtual string Description => "DefaultDescription";

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
        ///     Gets the possible commands for the Plugin.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The commands that the main module can call from the plugin.
        /// </value>
        public virtual List<Command> Commands => new();

        /// <inheritdoc />
        /// <summary>
        ///     Executes this instance.
        ///     Absolute necessary.
        /// </summary>
        /// <returns>
        ///     Status Code
        /// </returns>
        public abstract int Execute();

        /// <inheritdoc />
        /// <summary>
        ///     Executes the command.
        ///     Returns the result as object.
        ///     If we allow plugins, we must know what the plugin returns beforehand.
        ///     Based on the architecture say an image Viewer. The base module that handles most images is a plugin and always
        ///     returns a BitMapImage.
        ///     Every new plugin for Image viewing must nur return the same.
        ///     So if we add a plugin for another Image type, we define the plugin as Image Codec for example.
        ///     The main module now always expects a BitMapImage as return value.
        ///     This method is optional.
        /// </summary>
        /// <param name="id">The identifier of the command.</param>
        /// <returns>
        ///     Result object
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual object ExecuteCommand(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns the type of the plugin. Defined by the coder.
        ///     As already mentioned in ExecuteCommand, we need to know what we can expect as return value from this Plugin.
        ///     With this the main module can judge what to expect from the plugin.
        ///     This method is optional.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///     int as Id, can be used by the dev to define or get the type of Plugin this is
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual int GetPluginType(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the basic information of the plugin human readable.
        ///     This method is optional.
        /// </summary>
        /// <returns>
        ///     Info about the plugin
        /// </returns>
        public virtual string GetInfo()
        {
            // Default implementation
            return "Default plugin info";
        }

        /// <inheritdoc />
        /// <summary>
        ///     Closes this instance.
        ///     This method is optional.
        /// </summary>
        /// <returns>
        ///     Status Code
        /// </returns>
        public virtual int Close()
        {
            // Default implementation
            return 0;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Method to publish an event through the event aggregator.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventToPublish"></param>
        public void PublishEvent<TEvent>(TEvent eventToPublish)
        {
            EventAggregator?.Publish(eventToPublish);
        }
    }
}
