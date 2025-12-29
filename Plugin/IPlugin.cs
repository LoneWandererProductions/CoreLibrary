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
        ///     Gets or sets the event aggregator for the plugin.
        /// </summary>
        IEventAggregator EventAggregator { get; set; }

        /// <summary>
        ///     Gets the name. This field must be equal to the file name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type. This field is optional.
        /// </summary>
        string Type { get; }

        /// <summary>
        ///     Gets the description. This field is optional.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets the version. This field is optional.
        /// </summary>
        Version Version { get; }

        /// <summary>
        ///     Gets the possible commands for the Plugin. This field is optional.
        /// </summary>
        List<Command> Commands { get; }

        /// <summary>
        ///     Executes this instance. Absolute necessary.
        /// </summary>
        /// <returns>Status Code</returns>
        int Execute();

        /// <summary>
        ///     Executes the command. Returns the result as object. This method is optional.
        /// </summary>
        /// <param name="id">The identifier of the command.</param>
        /// <returns>Result object</returns>
        object ExecuteCommand(int id);

        /// <summary>
        ///     Returns the type of the plugin. Defined by the coder. This method is optional.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>int as Id</returns>
        int GetPluginType(int id);

        /// <summary>
        ///     Gets the basic information of the plugin human readable. This method is optional.
        /// </summary>
        /// <returns>Info about the plugin</returns>
        string GetInfo();

        /// <summary>
        ///     Closes this instance. This method is optional.
        /// </summary>
        /// <returns>Status Code</returns>
        int Close();

        /// <summary>
        ///     Method to publish an event through the event aggregator.
        /// </summary>
        void PublishEvent<TEvent>(TEvent eventToPublish);
    }
}
