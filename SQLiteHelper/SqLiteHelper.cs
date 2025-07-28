/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteHelper.cs
 * PURPOSE:     Helper Class for some Conversions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SqliteHelper;

/// <summary>
///     Helper Class
/// </summary>
internal static class SqliteHelper
{
    /// <summary>
    ///     Loads the CSV into Table conform format.
    /// </summary>
    /// <param name="csv">The CSV.</param>
    /// <param name="headers">if set to <c>true</c> [headers].</param>
    /// <returns>Table Set</returns>
    [return: MaybeNull]
    public static List<TableSet> LoadCsv(List<List<string>> csv, bool headers)
    {
        var cache = new List<List<string>>(csv);

        if (csv.Count == 0)
        {
            return null;
        }

        if (headers)
        {
            cache.RemoveAt(0);
        }

        var lst = new List<TableSet>(cache.Count);

        lst.AddRange(cache.Select(row => new TableSet(row)));

        return lst;
    }

    /// <summary>
    ///     Exports the CSV.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="info">The information.</param>
    /// <returns>Csv ready Format</returns>
    [return: MaybeNull]
    public static List<List<string>> ExportCsv(DataSet table, DictionaryTableColumns info)
    {
        if (table == null)
        {
            return null;
        }

        var lst = new List<List<string>>(table.Row.Count + 1);
        var cache = new List<string>(info.DColumns.Count);
        cache.AddRange(info.DColumns.Select(name => name.Key));
        lst.Add(cache);

        var second = Converge(table);
        lst.AddRange(second);

        return lst;
    }

    /// <summary>
    ///     Exports the CSV.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>Csv ready Format</returns>
    [return: MaybeNull]
    internal static List<List<string>> ExportCsv(DataSet table)
    {
        return table == null ? null : Converge(table);
    }

    /// <summary>
    ///     Converges the specified table.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <returns>Read into new Format</returns>
    private static List<List<string>> Converge(DataSet table)
    {
        var lst = new List<List<string>>(table.Row.Count);
        lst.AddRange(table.Row.Select(row => row.Row.ToList()));
        return lst;
    }
}
