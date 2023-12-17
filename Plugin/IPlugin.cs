/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/IPlugin.cs
 * PURPOSE:     Basic Plugin Support
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

// ReSharper disable UnusedParameter.Global, future proofing, it is up to the person how to use this ids
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    ///     Plugin Interface Implementation
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        ///     Gets the name.
        ///     This field must be equal to the file name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        string Name { get; }

        /// <summary>
        ///     Gets the type.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        string Type { get; }

        /// <summary>
        ///     Gets the description.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        string Description { get; }

        /// <summary>
        ///     Gets the version.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        Version Version { get; }

        /// <summary>
        ///     Gets the possible commands for the Plugin.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The commands that the main module can call from the plugin.
        /// </value>
        List<Command> Commands { get; }

        /// <summary>
        ///     Executes this instance.
        ///     Absolute necessary.
        /// </summary>
        /// <returns>Status Code</returns>
        int Execute();

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
        /// <returns>Status Code</returns>
        object ExecuteCommand(int id);

        /// <summary>
        ///     Returns the type of the plugin. Defined by the coder.
        ///     As already mentioned in ExecuteCommand, we need to know what we can expect as return value from this Plugin.
        ///     With this the main module can judge what to expect from the plugin.
        ///     This method is optional.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>int as Id, can be used by the dev to define or get the type of Plugin this is</returns>
        int GetPluginType(int id);

        /// <summary>
        ///     Gets the basic information of the plugin human readable.
        ///     This method is optional.
        /// </summary>
        /// <returns>
        ///     Info about the plugin
        /// </returns>
        string GetInfo();

        /// <summary>
        ///     Closes this instance.
        ///     This method is optional.
        /// </summary>
        /// <returns>Status Code</returns>
        int Close();
    }
}
