/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        PluginLoader/LoaderErrorEventArgs.cs
 * PURPOSE:     Basic error Logging Class, can and will be used for further external Communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace PluginLoader
{
    /// <inheritdoc />
    /// <summary>
    ///     Error Message Class for the Plugin Loader
    /// </summary>
    /// <seealso cref="T:System.EventArgs" />
    public sealed class LoaderErrorEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:PluginLoader.LoaderErrorEventArgs" /> class.
        /// </summary>
        /// <param name="error">The error message.</param>
        public LoaderErrorEventArgs(string error)
        {
            Error = error;
        }

        /// <summary>
        ///     Gets the error.
        /// </summary>
        /// <value>
        ///     The error.
        /// </value>
        public string Error { get; }
    }
}
