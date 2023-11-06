/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SqLiteGuiPlugin.cs
 * PURPOSE:     SqLiteGui Plugin Implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
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
