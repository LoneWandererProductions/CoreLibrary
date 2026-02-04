/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        TableObject.cs
 * PURPOSE:     Add new Tables
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using SqliteHelper;

namespace SQLiteGui
{
    /// <summary>
    ///     Helper Object to feed into Sqlite Helper
    /// </summary>
    /// <param name="Header">     Gets or sets the header. </param>
    /// <param name="Columns">     Gets or sets the columns. </param>
    internal sealed record TableObject(string Header, DictionaryTableColumns Columns)
    {
        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        public DictionaryTableColumns Columns { get; set; } = Columns;
    }
}
