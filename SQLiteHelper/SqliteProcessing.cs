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
using System.Linq;

namespace SqliteHelper;

/// <summary>
///     The SQLite processing class for handling database schema operations.
/// </summary>
internal static class SqliteProcessing
{
    // Constants for easier readability and better maintainability
    private const string PrimaryKey = "1";
    private const string NotNull = "1";
    private const string Unique = "1";

    /// <summary>
    ///     Converts the headers of a SQL table into a dictionary of column information.
    /// </summary>
    /// <param name="table">DataTable representing the table schema.</param>
    /// <returns>A dictionary where the key is the column name and the value contains the column information.</returns>
    internal static Dictionary<string, TableColumns>? ConvertTableHeaders(DataTable table)
    {
        var headers = new Dictionary<string, TableColumns>();

        foreach (DataRow row in table.Rows)
        {
            if (row == null)
            {
                continue;
            }

            var column = new TableColumns
            {
                NotNull = row.ItemArray[3].ToString() == NotNull,
                RowId = row.ItemArray[0]?.ToString(),
                PrimaryKey = row.ItemArray[5].ToString() == PrimaryKey,
                Unique = row.ItemArray[5].ToString() == Unique
            };

            column = SetDataType(column, row.ItemArray[2]?.ToString());

            if (column == null)
            {
                return null; // Early exit if type is invalid
            }

            headers.Add(row.ItemArray[1]?.ToString(), column);
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
        return table.Rows.Cast<DataRow>()
            .Select(row => row.ItemArray[1]?.ToString())
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
    }

    /// <summary>
    ///     Retrieves the name of the column from the index table.
    /// </summary>
    /// <param name="table">DataTable containing index information.</param>
    /// <returns>The name of the indexed column.</returns>
    internal static string GetTableHeader(DataTable table)
    {
        return table.Rows.Cast<DataRow>()
            .Select(row => row.ItemArray[2]?.ToString())
            .FirstOrDefault();
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
        foreach (var header in uniqueHeaders)
        {
            if (tableInfo.TryGetValue(header, out var value))
            {
                value.Unique = true;
            }
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
        return convert switch
        {
            SqLiteDataTypes.DateTime => DateTime.TryParse(value, out _),
            SqLiteDataTypes.Decimal => decimal.TryParse(value, out _),
            SqLiteDataTypes.Integer => int.TryParse(value, out _),
            SqLiteDataTypes.Real => double.TryParse(value, out _),
            SqLiteDataTypes.Text => true, // Text is always convertible from string
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
        return new DataSet
        {
            Row = dt.Rows.Cast<DataRow>()
                .Select(row => new TableSet { Row = row.ItemArray.Select(item => item?.ToString()).ToList() })
                .ToList()
        };
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

    private static TableColumns LogInvalidType(string type)
    {
        Trace.WriteLine($"{SqliteHelperResources.TraceCouldNotConvert}{type}");
        return null;
    }
}

// Extension method for TableColumns to set the data type.
