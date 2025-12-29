/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/TableRow.cs
 * PURPOSE:     Describes a Table Header
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace SqliteHelper
{
    /// <summary>
    ///     The table columns class.
    /// </summary>
    /// <summary>
    ///     Represents a column definition extracted from an SQLite table schema.
    /// </summary>
    public sealed class TableColumns
    {
        /// <summary>
        /// Gets the SQLite data type of the column.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public SqLiteDataTypes DataType { get; set; }

        /// <summary>
        /// Gets or sets whether the column value must be unique.
        /// Can be inferred by primary key or after index inspection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unique; otherwise, <c>false</c>.
        /// </value>
        public bool Unique { get; set; }

        /// <summary>
        /// Gets whether the column is defined as an SQLite PRIMARY KEY.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [primary key]; otherwise, <c>false</c>.
        /// </value>
        public bool PrimaryKey { get; init; }

        /// <summary>
        /// Gets whether the column is marked as NOT NULL.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [not null]; otherwise, <c>false</c>.
        /// </value>
        public bool NotNull { get; init; }

        /// <summary>
        /// Gets the internal SQLite ROWID position of the column if applicable.
        /// </summary>
        /// <value>
        /// The row identifier.
        /// </value>
        internal string? RowId { get; init; }

        /// <summary>
        /// Sets the SQLite type while returning the current instance.
        /// Enables fluent construction semantics.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public TableColumns WithDataType(SqLiteDataTypes type)
        {
            DataType = type;
            return this;
        }
    }

    /// <summary>
    ///     The dictionary table columns class.
    /// </summary>
    public sealed class DictionaryTableColumns
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DictionaryTableColumns" /> class.
        ///     Initiates DColumns as well
        /// </summary>
        public DictionaryTableColumns()
        {
            DColumns = new Dictionary<string, TableColumns>();
        }

        /// <summary>
        ///     Gets or sets the DColumns.
        /// </summary>
        public Dictionary<string, TableColumns> DColumns { get; internal init; }
    }
}
