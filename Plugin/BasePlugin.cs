/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/BasePlugin.cs
 * PURPOSE:     Basic abstract Plugin Implemnentation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

using System;
using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    /// Abstract Implementation of Plugin
    /// The user can pick and choose what he needs in a cleaner ways
    /// </summary>
    /// <seealso cref="Plugin.IPlugin" />
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Event that plugins can use to notify about certain actions.
        /// </summary>
        public event EventHandler<PluginEventArgs> PluginEventOccurred = delegate { };

        /// <summary>
        /// Raises the <see cref="E:PluginEventOccurred" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PluginEventArgs"/> instance containing the event data.</param>
        protected void OnPluginEventOccurred(PluginEventArgs e)
        {
            PluginEventOccurred?.Invoke(this, e);
        }

        // Default implementations for other IPlugin members

        /// <summary>
        /// Gets the name.
        /// This field must be equal to the file name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name => "DefaultPlugin";

        /// <summary>
        /// Gets the type.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual string Type => "DefaultType";

        /// <summary>
        /// Gets the description.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public virtual string Description => "DefaultDescription";

        /// <summary>
        /// Gets the version.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public virtual Version Version => new Version(1, 0);

        /// <summary>
        /// Gets the possible commands for the Plugin.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The commands that the main module can call from the plugin.
        /// </value>
        public virtual List<Command> Commands => new List<Command>();

        /// <summary>
        /// Executes this instance.
        /// Absolute necessary.
        /// </summary>
        /// <returns>
        /// Status Code
        /// </returns>
        public abstract int Execute();

        /// <summary>
        /// Executes the command.
        /// Returns the result as object.
        /// If we allow plugins, we must know what the plugin returns beforehand.
        /// Based on the architecture say an image Viewer. The base module that handles most images is a plugin and always
        /// returns a BitMapImage.
        /// Every new plugin for Image viewing must nur return the same.
        /// So if we add a plugin for another Image type, we define the plugin as Image Codec for example.
        /// The main module now always expects a BitMapImage as return value.
        /// This method is optional.
        /// </summary>
        /// <param name="id">The identifier of the command.</param>
        /// <returns>
        /// Result object
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual object ExecuteCommand(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the type of the plugin. Defined by the coder.
        /// As already mentioned in ExecuteCommand, we need to know what we can expect as return value from this Plugin.
        /// With this the main module can judge what to expect from the plugin.
        /// This method is optional.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// int as Id, can be used by the dev to define or get the type of Plugin this is
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual int GetPluginType(int id)
        {
            // Default implementation or throw NotImplementedException
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the basic information of the plugin human readable.
        /// This method is optional.
        /// </summary>
        /// <returns>
        /// Info about the plugin
        /// </returns>
        public virtual string GetInfo()
        {
            // Default implementation
            return "Default plugin info";
        }

        /// <summary>
        /// Closes this instance.
        /// This method is optional.
        /// </summary>
        /// <returns>
        /// Status Code
        /// </returns>
        public virtual int Close()
        {
            // Default implementation
            return 0;
        }
    }

}
