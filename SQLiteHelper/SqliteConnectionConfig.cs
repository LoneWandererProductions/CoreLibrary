/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteConnectionConfig.cs
 * PURPOSE:     Connection Strings
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;

namespace SqliteHelper
{
    /// <summary>
    ///     The sql lite connection Config class.
    /// </summary>
    internal static class SqliteConnectionConfig
    {
        /// <summary>
        ///     Gets or sets the location.
        /// </summary>
        internal static string? Location { get; set; }

        /// <summary>
        ///     Gets or sets the db name.
        /// </summary>
        internal static string? DbName { get; set; }

        /// <summary>
        ///     Basic Value is 3
        /// </summary>
        internal static int DbVersion { get; set; } = 3;

        /// <summary>
        ///     Basic Value is 3
        /// </summary>
        internal static int TimeOut { get; set; } = 3;

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        internal static string ConnectionString
            =>
                string.Concat(SqliteHelperResources.DataSource, FullPath, SqliteHelperResources.DataVersion,
                    DbVersion,
                    SqliteHelperResources.End);

        /// <summary>
        ///     Gets the full path.
        /// </summary>
        internal static string FullPath => Path.Combine(Location, DbName);
    }
}
