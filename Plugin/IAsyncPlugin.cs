/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/IAsyncPlugin.cs
 * PURPOSE:     Basic Plugin Support, in this case async
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */

// ReSharper disable UnusedParameter.Global, future proofing, it is up to the person how to use this ids
// ReSharper disable UnusedMember.Global


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin
{
    /// <summary>
    ///     Async version of the Plugin Interface
    /// </summary>
    public interface IAsyncPlugin
    {
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        string Name { get; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        string Type { get; }

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        string Description { get; }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        Version Version { get; }

        /// <summary>
        ///     Gets the commands.
        /// </summary>
        /// <value>
        ///     The commands.
        /// </value>
        List<Command> Commands { get; }

        /// <summary>
        ///     Executes the asynchronous.
        /// </summary>
        /// <returns>Status Code asnc</returns>
        Task<int> ExecuteAsync();

        /// <summary>
        ///     Executes the command asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Result object, async.</returns>
        Task<object> ExecuteCommandAsync(int id);

        /// <summary>
        ///     Gets the plugin type asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Status Code asnc</returns>
        Task<int> GetPluginTypeAsync(int id);

        /// <summary>
        ///     Gets the information.
        /// </summary>
        /// <returns>
        ///     Info about the plugin
        /// </returns>
        string GetInfo();

        /// <summary>
        ///     Closes asynchronous.
        /// </summary>
        /// <returns>Status Code</returns>
        Task<int> CloseAsync();
    }
}
