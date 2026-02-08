/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SqLiteGuiPlugin.cs
 * PURPOSE:     SqLiteGui Plugin Implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using Contracts;
using SqliteHelper;

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
        public string Name { get; } = nameof(SqliteUtility);

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
        ///     Executes this instance.
        /// </summary>
        /// <returns>Status</returns>
        public int Execute()
        {
            Win = new SqLiteGuiWindow();
            Win.Show();
            return 0;
        }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <returns>The Current Version</returns>
        private static Version GetVersion()
        {
            var assembly = typeof(SqliteUtility).Assembly;
            var assemblyName = assembly.GetName();

            return assemblyName.Version;
        }
    }
}
