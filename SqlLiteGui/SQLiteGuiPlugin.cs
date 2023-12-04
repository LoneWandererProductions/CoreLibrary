/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SqLiteGuiPlugin.cs
 * PURPOSE:     SqLiteGui Plugin Implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using Plugin;
using SQLiteHelper;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     Plugin Entry
    /// </summary>
    /// <seealso cref="IPlugin" />
    public class SqLiteGuiPlugin : IPlugin
    {
        /// <summary>
        ///     Gets or sets the win.
        /// </summary>
        /// <value>
        ///     The win.
        /// </value>
        private SqLiteGuiWindow Win { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; } = nameof(SqlLiteUtility);

        /// <inheritdoc />
        /// <summary>
        ///     Gets the type.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public string Type { get; } = SqLiteGuiResource.Type;

        /// <inheritdoc />
        /// <summary>
        ///     Gets the description.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; } = SqLiteGuiResource.Description;

        /// <inheritdoc />
        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public Version Version { get; } = GetVersion();

        /// <inheritdoc />
        /// <summary>
        ///     Gets the possible commands for the Plugin.
        ///     in this case zero.
        ///     This field is optional.
        /// </summary>
        /// <value>
        ///     The commands that the main module can call from the plugin.
        /// </value>
        public List<Command> Commands { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>Status</returns>
        public int Execute()
        {
            Win = new SqLiteGuiWindow();
            Win.Show();
            return 0;
        }

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
        ///     Status Code
        /// </returns>
        public object ExecuteCommand(int id)
        {
            return 0;
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
        public int GetPluginType(int id)
        {
            return 0;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns all infos about the plugin
        /// </summary>
        public string GetInfo()
        {
            return string.Concat(Type, Environment.NewLine, Version, Environment.NewLine, Description);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Closes this instance.
        /// </summary>
        /// <returns>
        ///     Status Code
        /// </returns>
        public int Close()
        {
            Win.Close();
            return 0;
        }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <returns>The Current Version</returns>
        private static Version GetVersion()
        {
            var assembly = typeof(SqlLiteUtility).Assembly;
            var assemblyName = assembly.GetName();

            return assemblyName.Version;
        }
    }
}
