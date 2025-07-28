/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/TableRow.cs
 * PURPOSE:     Describes a Table Header
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace SqliteHelper;

/// <summary>
///     The table columns class.
/// </summary>
public sealed class TableColumns
{
    /// <summary>
    ///     DataType
    /// </summary>
    public SqLiteDataTypes DataType { get; set; }

    /// <summary>
    ///     Optional
    /// </summary>
    public bool Unique { get; set; }

    /// <summary>
    ///     Optional
    /// </summary>
    public bool PrimaryKey { get; init; }

    /// <summary>
    ///     Optional
    ///     false is standard
    /// </summary>
    public bool NotNull { get; init; }

    /// <summary>
    ///     Optional
    /// </summary>
    internal string RowId { get; init; }
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
