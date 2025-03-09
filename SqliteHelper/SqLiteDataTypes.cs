/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqLiteDataTypes.cs
 * PURPOSE:     Enums of Types Sqlite Supports
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace SqliteHelper
{
    /// <summary>
    ///     SqlLite Data types
    ///     https://www.sqlite.org/datatype3.html
    /// </summary>
    public enum SqLiteDataTypes
    {
        /// <summary>
        ///     The Text Data Type = 0.
        /// </summary>
        Text = 0,

        /// <summary>
        ///     The DateTime Data Type = 1.
        /// </summary>
        DateTime = 1,

        /// <summary>
        ///     The Integer Data Type = 2.
        /// </summary>
        Integer = 2,

        /// <summary>
        ///     The Real Data Type = 3.
        /// </summary>
        Real = 3,

        /// <summary>
        ///     The Decimal Data Type = 4.
        /// </summary>
        Decimal = 4
    }
}