/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/TableSet.cs
 * PURPOSE:     Used in add Statement and Select
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqliteHelper;

/// <summary>
///     Used for the Add Statement
/// </summary>
public sealed class TableSet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TableSet" /> class.
    /// </summary>
    public TableSet()
    {
        Row = new List<string>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TableSet" /> class.
    /// </summary>
    /// <param name="lst">The list.</param>
    public TableSet(IEnumerable<string> lst)
    {
        Row = new List<string>(lst);
    }

    /// <summary>
    ///     Gets or sets the row.
    /// </summary>
    public List<string> Row { get; init; }
}

/// <summary>
///     Alternative View of the select unlike Data View for Data Binding, with some added extras
/// </summary>
public sealed class DataSet
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataSet" /> class.
    /// </summary>
    internal DataSet()
    {
        Row = new List<TableSet>();
    }

    /// <summary>
    ///     Gets the height.
    /// </summary>
    public int Height => Row.Count;

    /// <summary>
    ///     Gets the width.
    /// </summary>
    public int Width => Row[0].Row.Count;

    /// <summary>
    ///     Only Used for DataBinding
    /// </summary>
    public DataView Raw { get; internal set; }

    /// <summary>
    ///     Custom Data View,
    ///     Infos about the Table and the Data it Contains
    /// </summary>
    public List<TableSet> Row { get; internal init; }

    /// <summary>
    ///     Get a specific Column of the Data set
    /// </summary>
    /// <param name="height">Height of the Column</param>
    /// <returns>
    ///     Specific Column at that position, on Error return null.
    /// </returns>
    public List<string> Columns(int height)
    {
        if (height >= Height || height < 0)
        {
            return null;
        }

        return Row?[height].Row;
    }

    /// <summary>
    ///     Rows the specified width.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <returns>
    ///     Specific row at that position, on Error return null.
    /// </returns>
    public List<string> Rows(int width)
    {
        if (width >= Width || width < 0)
        {
            return null;
        }

        if (Row == null)
        {
            return null;
        }

        var lst = new List<string>(Width);

        lst.AddRange(from TableSet cell in Row
            let element = cell.Row[width]
            select element);
        return lst;
    }

    /// <summary>
    ///     Get a specific cell of the DataSet
    /// </summary>
    /// <param name="height">Height of the Column</param>
    /// <param name="width">Width of the Row</param>
    /// <returns>Specific element at that position</returns>
    public string Cell(int height, int width)
    {
        if (height >= Height || height < 0)
        {
            return null;
        }

        if (width >= Width || width < 0)
        {
            return null;
        }

        var lst = Row[height].Row;

        return lst[width];
    }
}
