/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/DataRegister.cs
 * PURPOSE:     Environment Data for use for the plugins
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    ///     Some plugins might need some stored data from the main module.
    ///     The Idea is we share this data over the Plugin Library and store in this Register.
    ///     The data is identified over an id and stored as object, so the plugin must know the id in question, which comes
    ///     from the Command Object, the data type must be known to the programmer of the plugin.
    ///     Still big room for improvement, but still better than using the clipboard or other means.
    /// </summary>
    public static class DataRegister
    {
        /// <summary>
        ///     Gets or sets the shared Data needed by the plugins.
        /// </summary>
        /// <value>
        ///     The data stored.
        /// </value>
        public static Dictionary<int, object> Store { get; set; }
    }
}
