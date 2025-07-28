/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteExecute.cs
 * PURPOSE:     Various Read and Write Operations for SqlLite
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;


// ReSharper disable ArrangeBraces_foreach

namespace SqliteHelper;

/// <summary>
///     Just execute our queries here
/// </summary>
internal sealed class SqliteExecute
{
    /// <summary>
    ///     Logging of System Messages
    /// </summary>
    private MessageItem _message;

    /// <summary>
    ///     Send our Message to the Subscribers
    /// </summary>
    public EventHandler<MessageItem> SetMessage { get; set; }

    /// <summary>
    ///     Switch to a new database Context
    /// </summary>
    /// <param name="location">Local Path to the Database</param>
    /// <param name="dbName">Name of the database</param>
    /// <returns>True if Connection is possible</returns>
    internal bool DatabaseContextSwitch(string location, string dbName)
    {
        MessageHandling.ClearErrors();
        var message = new MessageItem { Message = SqliteHelperResources.ContextSwitchLog, Level = 2 };
        OnError(message);

        SetDataBaseInfo(location, dbName);
        return CheckConnection();
    }

    /// <summary>
    ///     Creates an empty database file
    /// </summary>
    /// <param name="location">Local Path of the Database</param>
    /// <param name="dbName">Name of Database</param>
    /// <param name="overwrite">overwrite existing Database</param>
    /// <returns>Operation Success</returns>
    internal bool CreateDatabase(string location, string dbName, bool overwrite)
    {
        var check = GetDataBaseInfo(location, dbName, overwrite);

        //Do your work
        if (!check)
        {
            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.ErrorDbInfoCreate, overwrite), Level = 0
            };
            OnError(_message);
            return false;
        }

        if (!File.Exists(SqliteConnectionConfig.FullPath))
        {
            File.Create(SqliteConnectionConfig.FullPath).Dispose(); // Creates an empty file
        }

        _message = new MessageItem
        {
            Message = string.Concat(SqliteHelperResources.SuccessCreatedLog, dbName), Level = 2
        };
        OnError(_message);

        MessageHandling.ClearErrors();
        return true;
    }

    /// <summary>
    ///     Delete a database file
    /// </summary>
    /// <param name="location">Local Path of the Database</param>
    /// <param name="dbName">Name of Database</param>
    /// <returns>Operation Success</returns>
    internal bool DeleteDatabase(string location, string dbName)
    {
        var check = SetDataBaseInfo(location, dbName);
        //Do your work
        if (!check)
        {
            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.ErrorDbInfoDelete, location, dbName), Level = 0
            };
            OnError(_message);

            return false;
        }

        try
        {
            File.SetAttributes(SqliteConnectionConfig.FullPath, FileAttributes.Normal);
            File.Delete(SqliteConnectionConfig.FullPath);

            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.SuccessDeletedLog, dbName), Level = 2
            };
            OnError(_message);

            MessageHandling.ClearErrors();
        }
        catch (IOException ex)
        {
            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.ErrorDeleted, dbName,
                    SqliteHelperResources.Spacing, ex),
                Level = 0
            };
            OnError(_message);
        }

        return !File.Exists(SqliteConnectionConfig.FullPath);
    }

    /// <summary>
    ///     Rename the Table
    /// </summary>
    /// <param name="tblName">Name of the database</param>
    /// <param name="tblNameNew">New Name of the Database</param>
    /// <returns>Operation Success</returns>
    internal bool RenameTable(string tblName, string tblNameNew)
    {
        if (!CheckIfDatabaseTableExists(tblName))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return false;
        }

        if (CheckIfDatabaseTableExists(tblNameNew))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesAlreadyExist, Level = 0 };
            OnError(_message);

            return false;
        }

        var sqlQuery = SqliteQueryConst.RenameTable(tblName, tblNameNew);
        return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
    }

    /// <summary>
    ///     Attach Database
    /// </summary>
    /// <param name="dbName">Name of the Database</param>
    /// <param name="alias">New Name of the Database</param>
    /// <returns>Operation Success</returns>
    internal bool AttachDatabase(string dbName, string alias)
    {
        //Use current Connection, check if the current Database exists
        if (!File.Exists(SqliteConnectionConfig.FullPath))
        {
            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.ErrorDbNotFound,
                    SqliteConnectionConfig.FullPath),
                Level = 0
            };
            OnError(_message);

            return false;
        }

        //Check if Database we want to attach exists
        var toAttach = string.Concat(Environment.CurrentDirectory, Path.DirectorySeparatorChar, alias,
            SqliteHelperResources.SqlDbExt);

        if (!File.Exists(toAttach))
        {
            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.ErrorDbNotFound, toAttach), Level = 0
            };
            OnError(_message);

            return false;
        }

        //Do your work
        var sqlQuery = SqliteQueryConst.AttachDatabaseTable(dbName, alias);
        return ExecuteNonQuery(sqlQuery, false);
    }

    /// <summary>
    ///     Collect all data from the Master table
    /// </summary>
    /// <returns>All Info as string</returns>
    internal string GetDatabaseInfos()
    {
        var sqlQuery = SqliteQueryConst.GetDatabaseStatus();

        var dt = SelectDataTable(sqlQuery);

        return dt.Rows.Count == 0 ? null : GetAllData(dt);
    }

    /// <summary>
    ///     Checks if Table exists
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>Operation Success</returns>
    internal bool CheckIfDatabaseTableExists(string tableAlias)
    {
        var sqlQuery = SqliteQueryConst.SelectTable(tableAlias);
        return ExecuteNonQuery(sqlQuery, SqliteHelperResources.SuppressError);
    }

    /// <summary>
    ///     Deletes Table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>Operation Success</returns>
    internal bool DropTable(string tableAlias)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return false;
        }

        var sqlQuery = SqliteQueryConst.DropTable(tableAlias);
        return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
    }

    /// <summary>
    ///     Deletes Content of the table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>Operation Success</returns>
    internal bool TruncateTable(string tableAlias)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);
            return false;
        }

        var sqlQuery = SqliteQueryConst.TruncateTable(tableAlias);
        return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
    }

    /// <summary>
    ///     Creates a Table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <param name="tableHeaders">Name of the Table Headers</param>
    /// <returns>Operation Success</returns>
    internal bool CreateTable(string tableAlias, DictionaryTableColumns tableHeaders)
    {
        if (CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesAlreadyExist, Level = 0 };
            OnError(_message);
            return false;
        }

        var sqlQuery = SqliteQueryConst.CreateTable(tableAlias, tableHeaders);

        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorTableKeyConstraint, Level = 0 };
        OnError(_message);

        return false;
    }

    /// <summary>
    ///     Adds multiple/single line(s) into a Table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <param name="table">Multiple Entries</param>
    /// <param name="checking">Shall the Insert be checked</param>
    /// <returns>Operation Success</returns>
    internal bool InsertMultipleRow(string tableAlias, List<TableSet> table, bool checking)
    {
        var tableInfo = InternalPragmaTableInfo(tableAlias);

        //table doesn't exist?
        if (tableInfo == null)
        {
            _message = new MessageItem
            {
                Message = SqliteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
            };
            OnError(_message);
            return false;
        }

        //empty input
        if (table == null || table.Count == 0)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorEmptyInput, Level = 0 };
            OnError(_message);
            return false;
        }

        var headerTable = tableInfo.Keys.ToList();

        var sqlQuery = SqliteQueryConst.InsertTable(tableAlias, headerTable);

        //finish
        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteInsertQuery(sqlQuery, table, tableInfo, checking);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorInsertSingleRow, Level = 0 };
        OnError(_message);

        return false;
    }

    /// <summary>
    ///     Delete Rows of a table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <param name="where">Name of the column</param>
    /// <param name="value">Value</param>
    /// <returns>Rows deleted</returns>
    internal int DeleteRows(string tableAlias, string where, string value)
    {
        var sqlQuery = SqliteQueryConst.DeleteRows(tableAlias, where, value);

        //finish
        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteDeleteQuery(sqlQuery);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorDeleteRows, Level = 0 };
        OnError(_message);

        return -1;
    }

    /// <summary>
    ///     Update Table
    ///     pragma get headers
    ///     Sample:
    ///     "update table set total = total + 1 where value = 'Value'";
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
    /// <param name="where">identifier for where clause, always equal</param>
    /// <param name="value">string value</param>
    /// <param name="lst">Values as string in order</param>
    /// <returns>Count of rows updated</returns>
    internal int UpdateTable(string tableAlias, CompareOperator operators, string where, string value,
        IReadOnlyList<string> lst)
    {
        var tableInfo = InternalPragmaTableInfo(tableAlias);

        if (tableInfo == null)
        {
            _message = new MessageItem
            {
                Message = SqliteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
            };
            OnError(_message);

            return -1;
        }

        var headerTable = tableInfo.Keys.ToList();

        var sqlQuery = SqliteQueryConst.UpdateTable(tableAlias, headerTable, operators, where, value);

        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteUpdateQuery(sqlQuery, lst);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorUpdateTable, Level = 0 };
        OnError(_message);

        return -1;
    }

    /// <summary>
    ///     Drop Unique Index on Column
    ///     https://www.sqlite.org/lang_createindex.html
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <param name="column">Column Name</param>
    /// <param name="indexName">Name of the Index</param>
    /// <returns>Operation Success</returns>
    internal bool CreateUniqueIndex(string tableAlias, string column, string indexName)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return false;
        }

        var sqlQuery = SqliteQueryConst.CreateUniqueIndex(tableAlias, column, indexName);

        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorTableKeyConstraint, Level = 0 };
        OnError(_message);

        return false;
    }

    /// <summary>
    ///     Drop Unique Index on Column
    ///     https://www.sqlite.org/lang_createindex.html
    /// </summary>
    /// <param name="indexName">Name of the Index</param>
    /// <returns>Operation Success</returns>
    internal bool DropUniqueIndex(string indexName)
    {
        var sqlQuery = SqliteQueryConst.DropUniqueIndex(indexName);

        if (sqlQuery != SqliteHelperResources.ErrorCheck)
        {
            return ExecuteNonQuery(sqlQuery, SqliteHelperResources.DoNotSuppressError);
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorTableKeyConstraint, Level = 0 };
        OnError(_message);

        return false;
    }

    /// <summary>
    ///     Wrapper around InternalPragmaTableInfo
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>Converted Object for external Access</returns>
    internal DictionaryTableColumns PragmaTable_Info(string tableAlias)
    {
        return new DictionaryTableColumns { DColumns = InternalPragmaTableInfo(tableAlias) };
    }

    /// <summary>
    ///     Returns a List with Names of all Table Headers with the Unique Property
    ///     Uses two 2 Pragma
    ///     Pragma_index_list is the main Pragma
    ///     Pragma_index_info won't be exposed to the User, since he has no use for it
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>List of Names of all Table Headers with the Unique Property</returns>
    [return: MaybeNull]
    internal List<string> Pragma_index_list(string tableAlias)
    {
        var uniqueColumnsList = new List<string>();

        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return uniqueColumnsList;
        }

        var sqlQuery = SqliteQueryConst.Pragma_index_list(tableAlias);
        var table = SelectDataTable(sqlQuery);

        if (table == null)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableInfoNotFound, Level = 0 };
            OnError(_message);

            return null;
        }

        var tableInfo = SqliteProcessing.CheckUniqueTableHeaders(table);

        if (tableInfo.Count == 0)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorCheckUniqueTableHeaders, Level = 1 };
            OnError(_message);

            return new List<string>();
        }

        //Pragma_index_info
        foreach (var indexName in tableInfo)
        {
            sqlQuery = SqliteQueryConst.Pragma_index_info(indexName);
            table = SelectDataTable(sqlQuery);
            var columnName = SqliteProcessing.GetTableHeader(table);

            if (string.IsNullOrEmpty(columnName))
            {
                _message = new MessageItem { Message = SqliteHelperResources.ErrorGetTableHeader, Level = 0 };
                OnError(_message);

                return uniqueColumnsList;
            }

            uniqueColumnsList.Add(columnName);
        }

        return uniqueColumnsList;
    }

    /// <summary>
    ///     Returns a List with Names of all Table Headers with the Unique Property
    /// </summary>
    /// <returns>List of Names of all Table Headers</returns>
    [return: MaybeNull]
    internal List<string> GetTables()
    {
        var sqlQuery = SqliteQueryConst.GetTables();

        var dt = SelectDataTable(sqlQuery);
        if (dt == null || dt.Rows.Count == 0)
        {
            return null;
        }

        return GetTableHeaders(dt);
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <returns>Result Set</returns>
    [return: MaybeNull]
    internal DataSet SimpleSelect(string tableAlias)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(null, tableAlias, string.Empty,
            string.Empty, CompareOperator.None, string.Empty);

        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="header">Optional, select over specific headers</param>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <returns>Result Set</returns>
    [return: MaybeNull]
    internal DataSet SimpleSelect(List<string> header, string tableAlias)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(header, tableAlias, string.Empty,
            string.Empty, CompareOperator.None, string.Empty);

        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="header">Optional, select over specific headers</param>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <param name="where">Optional, where by table row</param>
    /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
    /// <param name="whereValue">Compare to</param>
    /// <returns>Result Set as TableSet</returns>
    [return: MaybeNull]
    internal DataSet SimpleSelect(List<string> header, string tableAlias,
        string where, CompareOperator operators, string whereValue)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(header, tableAlias, string.Empty,
            where, operators, whereValue);

        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <param name="where">Optional, where by table row</param>
    /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
    /// <param name="whereValue">Compare to</param>
    /// <returns>Result Set as TableSet</returns>
    [return: MaybeNull]
    internal DataSet SimpleSelect(string tableAlias, string where, CompareOperator operators,
        string whereValue)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(null, tableAlias, string.Empty,
            where, operators, whereValue);

        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="header">Optional, select over specific headers</param>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <param name="oderBy">Optional, order by header</param>
    /// <param name="where">Optional, where by table row</param>
    /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
    /// <param name="whereValue">Compare to</param>
    /// <returns>Result Set as TableSet</returns>
    [return: MaybeNull]
    internal DataSet SimpleSelect(List<string> header, string tableAlias, string oderBy,
        string where, CompareOperator operators, string whereValue)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(header, tableAlias, oderBy,
            where, operators, whereValue);

        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     Select all elements from the in Clause
    ///     http://www.sqlitetutorial.net/sqlite-in/
    /// </summary>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <param name="headers">Optional, select over specific headers</param>
    /// <param name="whereValue">Optional, where by table row</param>
    /// <param name="inClause">All in specifics</param>
    /// <param name="oderBy">Optional, order by header</param>
    /// <returns>TableMultipleSet, results as string</returns>
    [return: MaybeNull]
    internal DataSet SelectIn(string tableAlias, List<string> headers, string whereValue, List<string> inClause,
        string oderBy)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = Select(headers, tableAlias, string.Empty, string.Empty, CompareOperator.None, string.Empty);
        sqlQuery = SelectInClause(sqlQuery, whereValue, inClause, oderBy);
        return ExecSelect(sqlQuery);
    }

    /// <summary>
    ///     Used by all Five
    /// </summary>
    /// <param name="sqlQuery">Generated Query</param>
    /// <returns>Result Set as TableSet</returns>
    [return: MaybeNull]
    private DataSet ExecSelect(string sqlQuery)
    {
        if (sqlQuery == SqliteHelperResources.ErrorCheck)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorSimpleSelectParameters, Level = 0 };
            OnError(_message);

            return null;
        }

        var table = SelectMultipleDataTable(sqlQuery);
        if (table != null)
        {
            return table;
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorSimpleSelectExecution, Level = 0 };
        OnError(_message);

        return null;
    }

    /// <summary>
    ///     Checks if Connection is possible
    /// </summary>
    /// <returns>Operation Success</returns>
    private bool CheckConnection()
    {
        try
        {
            using var conn = new SQLiteConnection(SqliteConnectionConfig.ConnectionString);
            conn.Open(); // throws if invalid
        }
        catch (SQLiteException ex1) // Change exception type
        {
            _message = new MessageItem { Message = ex1.ToString(), Level = 0 };
            OnError(_message);

            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if Database doesn't exist
    ///     If it exists set your Connection
    /// </summary>
    /// <param name="location">Path of Database File</param>
    /// <param name="dbName">Name of Database</param>
    /// <returns>False if it doesn't exist</returns>
    private bool SetDataBaseInfo(string location, string dbName)
    {
        if (!File.Exists(location + Path.DirectorySeparatorChar + dbName))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorDatabaseAlreadyExists, Level = 0 };
            OnError(_message);

            return false;
        }

        SqliteConnectionConfig.Location = location;
        SqliteConnectionConfig.DbName = dbName;
        return true;
    }

    /// <summary>
    ///     Executes SqlLite Query
    ///     Mostly maintenance
    ///     With Rollback
    ///     Source:
    ///     https://www.sqlite.org/lang_transaction.html
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <param name="suppress">Suppress Error</param>
    /// <returns>Operation Success</returns>
    private bool ExecuteNonQuery(string sqlQuery, bool suppress)
    {
        //establish Connection
        using var conn = GetConn();
        if (conn == null)
        {
            return false;
        }

        //begin Transaction
        using var tr = conn.BeginTransaction();
        //Try to execute our stuff
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tr;
            cmd.CommandTimeout = SqliteConnectionConfig.TimeOut;
            cmd.CommandText = sqlQuery;
            cmd.ExecuteNonQuery();
            tr.Commit();

            _message = new MessageItem
            {
                Message = string.Concat(SqliteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
            };
            OnError(_message);
        }
        //catch error
        catch (SQLiteException ex1)
        {
            tr.Rollback();

            _message = new MessageItem { Message = string.Concat(ex1, sqlQuery), Level = 0 };
            if (!suppress)
            {
                OnError(_message);
            }

            return false;
        }

        return true;
    }

    /// <summary>
    ///     Returns Data table
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <returns>Result Table</returns>
    [return: MaybeNull]
    private DataTable SelectDataTable(string sqlQuery)
    {
        DataTable dt = new();

        try
        {
            using var conn = GetConn();
            if (conn == null)
            {
                return null;
            }

            using var command = new SQLiteCommand(sqlQuery, conn) { CommandTimeout = SqliteConnectionConfig.TimeOut };

            using var reader = command.ExecuteReader();

            dt.Load(reader); // Fills the DataTable

            _message = new MessageItem { Message = $"{SqliteHelperResources.SuccessExecutedLog}{sqlQuery}", Level = 2 };
            OnError(_message);
        }
        catch (SQLiteException ex)
        {
            _message = new MessageItem { Message = $"{ex}{sqlQuery}", Level = 0 };
            OnError(_message);
        }
        catch (NullReferenceException ex)
        {
            _message = new MessageItem
            {
                Message = $"{SqliteHelperResources.ErrorInSelectStatement}{sqlQuery}", Level = 1
            };
            OnError(_message);

            _message = new MessageItem { Message = ex.ToString(), Level = 0 };
            OnError(_message);
        }

        return dt;
    }

    /// <summary>
    ///     Selects a Table with a specific result output
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <returns>Result Table</returns>
    [return: MaybeNull]
    private DataSet SelectMultipleDataTable(string sqlQuery)
    {
        var dt = SelectDataTable(sqlQuery);
        if (dt == null || dt.Rows.Count == 0)
        {
            return null;
        }

        var tm = SqliteProcessing.ConvertToTableMultipleSet(dt);

        //for Data binding!
        tm.Raw = dt.DefaultView;
        return tm;
    }

    /// <summary>
    ///     Insert Content into the Table
    ///     Alt: Slightly more per-formant
    ///     INSERT INTO 'tableName' SELECT 'data1'
    ///     AS 'column1', 'data2'
    ///     AS 'column2'
    ///     UNION
    ///     SELECT 'data1', 'data2'
    ///     UNION
    ///     SELECT 'data1', 'data2'
    ///     UNION
    ///     SELECT 'data1', 'data2'
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <param name="table">Content to be added</param>
    /// <param name="tableInfo">Headers, Data Type and Constraints of the table</param>
    /// <param name="checking">Shall we check the input</param>
    /// <returns>Success Status</returns>
    private bool ExecuteInsertQuery(
        string sqlQuery,
        IReadOnlyCollection<TableSet> table,
        Dictionary<string, TableColumns> tableInfo,
        bool checking)
    {
        // Establish connection
        using var conn = GetConn();
        if (conn == null)
        {
            return false;
        }

        // Begin transaction
        using var tr = conn.BeginTransaction();


        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tr;
            cmd.CommandTimeout = SqliteConnectionConfig.TimeOut;
            cmd.CommandText = sqlQuery;

            var checkCount = 0;
            var paramCache = new Dictionary<int, SQLiteParameter>();

            var syntax = new SqliteSyntax(SetMessage);

            foreach (var row in table)
            {
                if (checking)
                {
                    if (checkCount >= tableInfo.Count)
                    {
                        checkCount = 0;
                    }

                    var convert = tableInfo.ElementAt(checkCount).Value;
                    if (!syntax.AdvancedSyntaxCheck(tableInfo, table, row, convert))
                    {
                        return false;
                    }

                    checkCount++;
                }

                cmd.Parameters.Clear(); // Ensure no leftover parameters

                for (var i = 0; i < row.Row.Count; i++)
                {
                    if (!paramCache.TryGetValue(i, out var param))
                    {
                        param = new SQLiteParameter($"{SqliteHelperResources.Param}{i}", row.Row[i]);
                        cmd.Parameters.Add(param);
                        paramCache[i] = param;
                    }
                    else
                    {
                        param.Value = row.Row[i];
                    }
                }

                cmd.ExecuteNonQuery();
            }

            tr.Commit();

            _message = new MessageItem { Message = $"{SqliteHelperResources.SuccessExecutedLog}{sqlQuery}", Level = 2 };
            OnError(_message);

            return true;
        }
        catch (SQLiteException ex)
        {
            tr.Rollback();

            _message = new MessageItem { Message = $"{ex}{sqlQuery}", Level = 0 };
            OnError(_message);
        }

        return false;
    }


    /// <summary>
    ///     Update Table
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <param name="lst">Content to be added</param>
    /// <returns>
    ///     Count of rows
    /// </returns>
    private int ExecuteUpdateQuery(string sqlQuery, IReadOnlyList<string> lst)
    {
        var affectedRows = -1;

        // Establish connection
        using var conn = GetConn();
        if (conn == null)
        {
            return -1;
        }

        // Begin transaction
        using var tr = conn.BeginTransaction();

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tr;
            cmd.CommandTimeout = SqliteConnectionConfig.TimeOut;
            cmd.CommandText = sqlQuery;

            // Use parameter caching for efficiency
            var paramCache = new Dictionary<int, SQLiteParameter>();

            for (var i = 0; i < lst.Count; i++)
            {
                if (!paramCache.TryGetValue(i, out var param))
                {
                    param = new SQLiteParameter($"{SqliteHelperResources.Param}{i}", lst[i]);
                    cmd.Parameters.Add(param);
                    paramCache[i] = param;
                }
                else
                {
                    param.Value = lst[i];
                }
            }

            affectedRows = cmd.ExecuteNonQuery();
            tr.Commit();

            _message = new MessageItem { Message = $"{SqliteHelperResources.SuccessExecutedLog}{sqlQuery}", Level = 2 };
            OnError(_message);
        }
        catch (SQLiteException ex)
        {
            tr.Rollback();

            _message = new MessageItem { Message = $"{ex}{sqlQuery}", Level = 0 };
            OnError(_message);
        }

        return affectedRows;
    }

    /// <summary>
    ///     Delete Entry
    /// </summary>
    /// <param name="sqlQuery">Query Text</param>
    /// <returns>Count of rows</returns>
    private int ExecuteDeleteQuery(string sqlQuery)
    {
        var affectedRows = -1;

        // Establish connection
        using var conn = GetConn();
        if (conn == null)
        {
            return -1;
        }

        // Begin transaction
        using var tr = conn.BeginTransaction();

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tr;
            cmd.CommandTimeout = SqliteConnectionConfig.TimeOut;
            cmd.CommandText = sqlQuery;

            affectedRows = cmd.ExecuteNonQuery();
            tr.Commit();

            _message = new MessageItem { Message = $"{SqliteHelperResources.SuccessExecutedLog}{sqlQuery}", Level = 2 };
            OnError(_message);
        }
        catch (SQLiteException ex)
        {
            tr.Rollback();

            _message = new MessageItem { Message = $"{ex}{sqlQuery}", Level = 0 };
            OnError(_message);
        }

        return affectedRows;
    }

    /// <summary>
    ///     A simple basic select
    ///     https://www.sqlite.org/lang_select.html
    ///     With extensions
    /// </summary>
    /// <param name="headers">Optional, select over specific headers</param>
    /// <param name="tableAlias">Must Name of Table</param>
    /// <param name="oderBy">Optional, order by header</param>
    /// <param name="where">Optional, where by table row</param>
    /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
    /// <param name="whereValue">Compare to</param>
    /// <returns>TableMultipleSet, results as string</returns>
    [return: MaybeNull]
    private string Select(IReadOnlyCollection<string> headers, string tableAlias, string oderBy,
        string where, CompareOperator operators, string whereValue)
    {
        var tableInfo = InternalPragmaTableInfo(tableAlias);

        if (tableInfo == null)
        {
            _message = new MessageItem
            {
                Message = SqliteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
            };
            OnError(_message);

            return null;
        }

        var headerTable = tableInfo.Keys.ToList();

        List<string> header = null;
        if (headers != null)
        {
            header = new List<string>(headers);
        }

        return SqliteQueryConst.SimpleSelect(headerTable, header, tableAlias, oderBy, where, operators,
            whereValue);
    }

    /// <summary>
    ///     Sql Query with in Clause
    /// </summary>
    /// <param name="sqlQuery">Select Statement</param>
    /// <param name="whereValue">Optional, where by table row</param>
    /// <param name="inClause">In values</param>
    /// <param name="oderBy">Optional, order by header</param>
    /// <returns>Completed Sql Query</returns>
    private static string SelectInClause(string sqlQuery, string whereValue, IReadOnlyList<string> inClause,
        string oderBy)
    {
        sqlQuery = string.Concat(sqlQuery,
            SqliteHelperResources.Spacing,
            SqliteHelperResources.SqlWhere,
            SqliteHelperResources.Spacing,
            whereValue, SqliteHelperResources.Spacing,
            SqliteHelperResources.SqlIn,
            SqliteHelperResources.BracketOpen);

        for (var i = 0; i < inClause.Count - 1; i++)
        {
            sqlQuery = string.Concat(sqlQuery, inClause[i], SqliteHelperResources.Comma);
        }

        sqlQuery = string.Concat(sqlQuery, inClause[^1], SqliteHelperResources.BracketClose);

        if (oderBy == null)
        {
            return sqlQuery;
        }

        return string.Concat(sqlQuery, SqliteHelperResources.Spacing, SqliteHelperResources.SqlOrderBy,
            SqliteHelperResources.Spacing, oderBy);
    }

    /// <summary>
    ///     Converts Table results to query
    /// </summary>
    /// <param name="dt">Database Query results</param>
    /// <returns>String of DB Status</returns>
    private static string GetAllData(DataTable dt)
    {
        var str = string.Concat(SqliteHelperResources.MessageInitiate, Environment.NewLine);

        foreach (DataRow row in dt.Rows)
        {
            for (var i = 0; i < row.ItemArray.Length; i++)
            {
                str = string.Concat(str, SqliteHelperResources.Spacing, row[i], Environment.NewLine);
            }
        }

        return str;
    }

    /// <summary>
    ///     Convert query to List String
    /// </summary>
    /// <param name="dt">Data Table of the select Statement</param>
    /// <returns>List of Names of all Table Headers</returns>
    private static List<string> GetTableHeaders(DataTable dt)
    {
        return (from DataRow row in dt.Rows select row[0].ToString()).ToList();
    }

    /// <summary>
    ///     Collect all Data needed about a Table to add Elements
    ///     Uses Pragma_index_list
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    /// <returns>Headers, Data Type and Constraints of the table</returns>
    [return: MaybeNull]
    private Dictionary<string, TableColumns> InternalPragmaTableInfo(string tableAlias)
    {
        if (!CheckIfDatabaseTableExists(tableAlias))
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableDoesNotExist, Level = 0 };
            OnError(_message);

            return null;
        }

        var sqlQuery = SqliteQueryConst.Pragma_TableInfo(tableAlias);

        var table = SelectDataTable(sqlQuery);
        if (table == null)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorTableInfoNotFound, Level = 0 };
            OnError(_message);

            return null;
        }

        var tableInfo = SqliteProcessing.ConvertTableHeaders(table);
        if (tableInfo == null)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorConvertTableInfos, Level = 0 };
            OnError(_message);

            return null;
        }

        //other Master Procedure
        var tableHeaders = Pragma_index_list(tableAlias);

        if (tableHeaders == null)
        {
            _message = new MessageItem { Message = SqliteHelperResources.ErrorPragmaIndexList, Level = 0 };
            OnError(_message);

            return null;
        }

        tableInfo = SqliteProcessing.AddUniqueStatus(tableInfo, tableHeaders);

        if (tableInfo != null)
        {
            return tableInfo;
        }

        _message = new MessageItem { Message = SqliteHelperResources.ErrorAddUniqueStatus, Level = 0 };
        OnError(_message);

        return null;
    }

    /// <summary>
    ///     Checks if Database doesn't exist
    ///     If it does not exist set Connection
    /// </summary>
    /// <param name="location">Path of Database File</param>
    /// <param name="dbName">Name of Database</param>
    /// <param name="overwrite">overwrite existing Database</param>
    /// <returns>False if it exists</returns>
    private static bool GetDataBaseInfo(string location, string dbName, bool overwrite)
    {
        //Database exists and we are not allowed to overwrite, Return
        if (File.Exists(Path.Combine(location, dbName)) && !overwrite)
        {
            return false;
        }

        SqliteConnectionConfig.Location = location;
        SqliteConnectionConfig.DbName = dbName;
        return true;
    }

    /// <summary>
    ///     Creates a connection with our database file.
    ///     If something went wrong we send null, this will have to be handled by the Queries that try to use this connection
    /// </summary>
    /// <returns>Connection or null, if we couldn't create a connection</returns>
    [return: MaybeNull]
    private SQLiteConnection GetConn()
    {
        try
        {
            var connection = new SQLiteConnection(SqliteConnectionConfig.ConnectionString);
            connection.Open();
            return connection;
        }
        catch (BadImageFormatException ex) // Incorrect architecture (e.g., x86 vs x64)
        {
            _message = new MessageItem
            {
                Message =
                    $"BadImageFormatException: {ex.Message} | ConnectionString: {SqliteConnectionConfig.ConnectionString}",
                Level = 0
            };
            OnError(_message);
        }
        catch (DllNotFoundException ex) // Missing SQLite native library
        {
            _message = new MessageItem
            {
                Message =
                    $"DllNotFoundException: {ex.Message} | ConnectionString: {SqliteConnectionConfig.ConnectionString}",
                Level = 0
            };
            OnError(_message);
        }
        catch (SQLiteException ex) // General SQLite errors
        {
            _message = new MessageItem
            {
                Message =
                    $"SQLiteException: {ex.Message} | ConnectionString: {SqliteConnectionConfig.ConnectionString}",
                Level = 0
            };
            OnError(_message);
        }
        catch (Exception ex) // Catch-all for unexpected errors
        {
            _message = new MessageItem
            {
                Message =
                    $"Unexpected Exception: {ex.Message} | ConnectionString: {SqliteConnectionConfig.ConnectionString}",
                Level = 0
            };
            OnError(_message);
        }

        return null;
    }

    /// <summary>
    ///     Inform Subscribers about the News
    /// </summary>
    /// <param name="dbMessage">Message</param>
    private void OnError(MessageItem dbMessage)
    {
        SetMessage?.Invoke(this, dbMessage);
    }
}
