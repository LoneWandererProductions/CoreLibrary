/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqLiteDataTypes.cs
 * PURPOSE:     Enums of Types Sqlite Supports
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqliteHelper;

internal sealed class SqliteSyntax
{
    /// <summary>
    ///     Send our Message to the Subscribers
    /// </summary>
    private readonly EventHandler<MessageItem> _setMessage;

    /// <summary>
    ///     Logging of System Messages
    /// </summary>
    private MessageItem _message;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteSyntax" /> class.
    /// </summary>
    /// <param name="setMessage">The set message.</param>
    public SqliteSyntax(EventHandler<MessageItem> setMessage = null)
    {
        _setMessage = setMessage;
    }

    /// <summary>
    ///     Basic sanity checks
    /// </summary>
    /// <param name="tableInfo">Headers, Data Type and Constraints of the table</param>
    /// <param name="table">Table Collection</param>
    /// <param name="row">Table Collection</param>
    /// <param name="convert">Table Collection</param>
    /// <returns>true if no errors were found</returns>
    internal bool AdvancedSyntaxCheck(ICollection tableInfo, IEnumerable<TableSet> table, TableSet row,
        TableColumns convert)
    {
        if (tableInfo.Count != table.First().Row.Count)
        {
            LogError(SqliteHelperResources.ErrorMoreElementsToAddThanRows);
            return false;
        }

        foreach (var rows in row.Row)
        {
            if (!CheckNullability(rows, convert))
            {
                return false;
            }

            if (!CheckTypeCompatibility(rows, convert))
            {
                return false;
            }

            if (!CheckUniqueness(row, convert))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Checks the nullability.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="convert">The convert.</param>
    /// <returns>Condition fulfilled</returns>
    private bool CheckNullability(object rows, TableColumns convert)
    {
        if (rows != null || convert.NotNull)
        {
            return true;
        }

        LogError(SqliteHelperResources.ErrorNotNullAble);
        return false;
    }

    /// <summary>
    ///     Checks the type compatibility.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="convert">The convert.</param>
    /// <returns>Condition fulfilled</returns>
    private bool CheckTypeCompatibility(string rows, TableColumns convert)
    {
        var check = SqliteProcessing.CheckConvert(convert.DataType, rows);
        if (check)
        {
            return true;
        }

        LogError($"{SqliteHelperResources.ErrorWrongType} - Value: {rows}, Expected Type: {convert.DataType}");
        return false;
    }

    /// <summary>
    ///     Checks the uniqueness.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="convert">The convert.</param>
    /// <returns></returns>
    private bool CheckUniqueness(TableSet row, TableColumns convert)
    {
        if (!convert.PrimaryKey && !convert.Unique)
        {
            return true;
        }

        var isUnique = row.Row.Distinct().Count() == row.Row.Count;
        if (isUnique)
        {
            return true;
        }

        LogError(SqliteHelperResources.ErrorNotUnique);
        return false;
    }

    /// <summary>
    ///     Logs the error.
    /// </summary>
    /// <param name="message">The message.</param>
    private void LogError(string message)
    {
        _message = new MessageItem { Message = message, Level = 0 };
        OnError(_message);
    }

    /// <summary>
    ///     Inform Subscribers about the News
    /// </summary>
    /// <param name="dbMessage">Message</param>
    private void OnError(MessageItem dbMessage)
    {
        _setMessage?.Invoke(this, dbMessage);
    }
}
