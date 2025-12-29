/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/Config.cs
 * PURPOSE:     Config Object of our Wrapper
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace SqliteHelper
{
    /// <summary>
    ///     Collection of generic Connection Infos
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        ///     Gets or sets the location.
        /// </summary>
        public string? Location { get; internal init; }

        /// <summary>
        ///     Gets or sets the Database name.
        /// </summary>
        public string? DbName { get; internal init; }

        /// <summary>
        ///     Gets or sets the time out.
        /// </summary>
        public int TimeOut { get; internal init; }

        /// <summary>
        ///     Gets or sets the db version.
        /// </summary>
        public int DbVersion { get; internal init; }

        /// <summary>
        ///     Gets or sets the last Errors.
        /// </summary>
        public string LastError { get; internal init; }

        /// <summary>
        ///     Gets or sets the List Errors.
        /// </summary>
        public List<string> ListErrors { get; internal init; }

        /// <summary>
        ///     Gets or sets the max lines error.
        /// </summary>
        public int MaxLinesError { get; internal init; }

        /// <summary>
        ///     Gets or sets the max lines log.
        /// </summary>
        public int MaxLinesLog { get; internal init; }
    }
}
