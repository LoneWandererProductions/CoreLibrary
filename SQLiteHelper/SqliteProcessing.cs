/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteProcessing.cs
 * PURPOSE:     Processing of various Database Objects processing and Checks
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace SqliteHelper;

/// <summary>
///     The SQLite processing class for handling database schema operations.
/// </summary>
internal static class SqliteProcessing
{
    /// <summary>
    ///     Converts the headers of a SQL table into a dictionary of column information.
    /// </summary>
    /// <param name="table">DataTable representing the table schema.</param>
    /// <returns>A dictionary where the key is the column name and the value contains the column information.</returns>
    internal static Dictionary<string, TableColumns>? ConvertTableHeaders(DataTable table)
    {
        var headers = new Dictionary<string, TableColumns>(table.Rows.Count, StringComparer.Ordinal);

        foreach (DataRow row in table.Rows)
        {
            if (row == null) continue;

            string? name = row.Field<string>(1);
            if (string.IsNullOrEmpty(name))
                continue;

            var column = new TableColumns
            {
                RowId = row[0]?.ToString(),
                NotNull = row.Field<long?>(3) == 1,
                PrimaryKey = row.Field<long?>(5) == 1
            };

            // Extract type and normalize
            string? type = row.Field<string>(2);
            column = SetDataType(column, type);

            if (column == null)
                return null; // bail-out on invalid type

            // Unique is not reliable here, only PRIMARY KEY reliably marks uniqueness.
            column.Unique = column.PrimaryKey;

            headers[name] = column;
        }

        return headers;
    }

    /// <summary>
    ///     Extracts unique column headers from the table's index data.
    /// </summary>
    /// <param name="table">DataTable containing index information.</param>
    /// <returns>A list of unique column names.</returns>
    internal static List<string> CheckUniqueTableHeaders(DataTable table)
    {
        var uniqueCols = new List<string>(table.Rows.Count);

        foreach (DataRow row in table.Rows)
        {
            string? col = row.Field<string>(1);

            if (!string.IsNullOrEmpty(col))
                uniqueCols.Add(col);
        }

        return uniqueCols;
    }

    /// <summary>
    ///     Retrieves the name of the column from the index table.
    /// </summary>
    /// <param name="table">DataTable containing index information.</param>
    /// <returns>The name of the indexed column.</returns>
    internal static string? GetTableHeader(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            string? name = row.Field<string>(2);
            if (!string.IsNullOrEmpty(name))
                return name;
        }
        return null;
    }

    /// <summary>
    ///     Adds the unique status to the table columns based on the provided unique headers.
    /// </summary>
    /// <param name="tableInfo">A dictionary containing the table columns.</param>
    /// <param name="uniqueHeaders">A list of unique column names.</param>
    /// <returns>The updated table information dictionary with unique status applied.</returns>
    internal static Dictionary<string, TableColumns> AddUniqueStatus(
        Dictionary<string, TableColumns> tableInfo,
        List<string> uniqueHeaders)
    {
        foreach (string header in uniqueHeaders)
        {
            if (tableInfo.TryGetValue(header, out var col))
                col.Unique = true; // mark as unique if indexed
        }

        return tableInfo;
    }

    /// <summary>
    ///     Checks if a value of one type can be converted to another type.
    /// </summary>
    /// <param name="convert">The target SQLite data type to check against.</param>
    /// <param name="value">The value to convert, expressed as a string.</param>
    /// <returns>True if the value can be converted to the target type, otherwise false.</returns>
    internal static bool CheckConvert(SqLiteDataTypes convert, string value)
    {
        if (value == null) return false;

        return convert switch
        {
            SqLiteDataTypes.DateTime => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            SqLiteDataTypes.Decimal => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
            SqLiteDataTypes.Integer => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            SqLiteDataTypes.Real => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
            SqLiteDataTypes.Text => true,
            _ => false
        };
    }

    /// <summary>
    ///     Converts a DataTable to a custom formatted DataSet.
    /// </summary>
    /// <param name="dt">The DataTable to convert.</param>
    /// <returns>A DataSet containing the converted table data.</returns>
    internal static DataSet ConvertToTableMultipleSet(DataTable dt)
    {
        var result = new DataSet { Row = new List<TableSet>(dt.Rows.Count) };

        foreach (DataRow row in dt.Rows)
        {
            var set = new TableSet { Row = new List<string?>(row.ItemArray.Length) };

            foreach (object? item in row.ItemArray)
                set.Row.Add(item?.ToString());

            result.Row.Add(set);
        }

        return result;
    }

    /// <summary>
    ///     Sets the data type for a TableColumns object based on the provided type string.
    /// </summary>
    /// <param name="column">The TableColumns object to update.</param>
    /// <param name="type">The type string from the SQLite schema.</param>
    /// <returns>The updated TableColumns object, or null if the type is invalid.</returns>
    private static TableColumns? SetDataType(TableColumns column, string type)
    {
        return type.ToLower() switch
        {
            SqliteHelperResources.SqlLiteDataTypeInteger => column.WithDataType(SqLiteDataTypes.Integer),
            SqliteHelperResources.SqlLiteDataTypeDecimal => column.WithDataType(SqLiteDataTypes.Decimal),
            SqliteHelperResources.SqlLiteDataTypeDateTime => column.WithDataType(SqLiteDataTypes.DateTime),
            SqliteHelperResources.SqlLiteDataTypeReal => column.WithDataType(SqLiteDataTypes.Real),
            SqliteHelperResources.SqlLiteDataTypeText => column.WithDataType(SqLiteDataTypes.Text),
            _ => LogInvalidType(type) // Log invalid type and return null
        };
    }

    /// <summary>
    /// Logs the type of the invalid.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Null or TableColumns.</returns>
    private static TableColumns? LogInvalidType(string type)
    {
        Trace.WriteLine($"{SqliteHelperResources.TraceCouldNotConvert}{type}");
        return null;
    }
}
