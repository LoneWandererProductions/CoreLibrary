/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SQLiteHelpers.cs
 * PURPOSE:     Various Read and Write Operations for SqlLite
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

// ReSharper disable ArrangeBraces_foreach

namespace SQLiteHelper
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

    /// <summary>
    ///     Compare Operators
    /// </summary>
    public enum CompareOperator
    {
        /// <summary>
        ///     None = 0.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The Equal Operator = 1.
        /// </summary>
        Equal = 1,

        /// <summary>
        ///     The Not equal Operator= 2.
        /// </summary>
        NotEqual = 2,

        /// <summary>
        ///     The Like Operator = 3.
        /// </summary>
        Like = 3,

        /// <summary>
        ///     The Not like Operator = 4.
        /// </summary>
        NotLike = 4
    }

    /// <summary>
    ///     Just execute our queries here
    /// </summary>
    internal sealed class SqlLiteExecute
    {
        /// <summary>
        ///     Logging of System Messages
        /// </summary>
        private MessageItem _message;

        /// <summary>
        ///     Send our Message to the Subscribers
        /// </summary>
        public EventHandler<MessageItem> setMessage;

        /// <summary>
        ///     Switch to a new database Context
        /// </summary>
        /// <param name="location">Local Path to the Database</param>
        /// <param name="dbName">Name of the database</param>
        /// <returns>True if Connection is possible</returns>
        internal bool DatabaseContextSwitch(string location, string dbName)
        {
            MessageHandling.ClearErrors();
            var message = new MessageItem {Message = SqLiteHelperResources.ContextSwitchLog, Level = 2};
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
                    Message = string.Concat(SqLiteHelperResources.ErrorDbInfoCreate, overwrite), Level = 0
                };
                OnError(_message);
                return false;
            }

            SQLiteConnection.CreateFile(SqlLiteConnectionConfig.FullPath);

            _message = new MessageItem
            {
                Message = string.Concat(SqLiteHelperResources.SuccessCreatedLog, dbName), Level = 2
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
                    Message = string.Concat(SqLiteHelperResources.ErrorDbInfoDelete, location, dbName), Level = 0
                };
                OnError(_message);

                return false;
            }

            try
            {
                File.SetAttributes(SqlLiteConnectionConfig.FullPath, FileAttributes.Normal);
                File.Delete(SqlLiteConnectionConfig.FullPath);

                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.SuccessDeletedLog, dbName), Level = 2
                };
                OnError(_message);

                MessageHandling.ClearErrors();
            }
            catch (IOException ex)
            {
                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.ErrorDeleted, dbName,
                        SqLiteHelperResources.Spacing, ex),
                    Level = 0
                };
                OnError(_message);
            }

            return !File.Exists(SqlLiteConnectionConfig.FullPath);
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);

                return false;
            }

            if (CheckIfDatabaseTableExists(tblNameNew))
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesAlreadyExist, Level = 0};
                OnError(_message);

                return false;
            }

            var sqlQuery = SqlLiteQueryConst.RenameTable(tblName, tblNameNew);
            return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
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
            if (!File.Exists(SqlLiteConnectionConfig.FullPath))
            {
                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.ErrorDbNotFound,
                        SqlLiteConnectionConfig.FullPath),
                    Level = 0
                };
                OnError(_message);

                return false;
            }

            //Check if Database we want to attach exists
            var toAttach = string.Concat(Environment.CurrentDirectory, Path.DirectorySeparatorChar, alias,
                SqLiteHelperResources.SqlDbExt);

            if (!File.Exists(toAttach))
            {
                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.ErrorDbNotFound, toAttach), Level = 0
                };
                OnError(_message);

                return false;
            }

            //Do your work
            var sqlQuery = SqlLiteQueryConst.AttachDatabaseTable(dbName, alias);
            return ExecuteNonQuery(sqlQuery, false);
        }

        /// <summary>
        ///     Collect all data from the Master table
        /// </summary>
        /// <returns>All Info as string</returns>
        internal string GetDatabaseInfos()
        {
            var sqlQuery = SqlLiteQueryConst.GetDatabaseStatus();

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
            var sqlQuery = SqlLiteQueryConst.SelectTable(tableAlias);
            return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.SuppressError);
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);

                return false;
            }

            var sqlQuery = SqlLiteQueryConst.DropTable(tableAlias);
            return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);
                return false;
            }

            var sqlQuery = SqlLiteQueryConst.TruncateTable(tableAlias);
            return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesAlreadyExist, Level = 0};
                OnError(_message);
                return false;
            }

            var sqlQuery = SqlLiteQueryConst.CreateTable(tableAlias, tableHeaders);

            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableKeyConstraint, Level = 0};
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
                    Message = SqLiteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
                };
                OnError(_message);
                return false;
            }

            //empty input
            if (table == null || table.Count == 0)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorEmptyInput, Level = 0};
                OnError(_message);
                return false;
            }

            var headerTable = tableInfo.Keys.ToList();

            var sqlQuery = SqlLiteQueryConst.InsertTable(tableAlias, headerTable);

            //finish
            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteInsertQuery(sqlQuery, table, tableInfo, checking);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorInsertSingleRow, Level = 0};
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
            var sqlQuery = SqlLiteQueryConst.DeleteRows(tableAlias, where, value);

            //finish
            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteDeleteQuery(sqlQuery);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorDeleteRows, Level = 0};
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
                    Message = SqLiteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
                };
                OnError(_message);

                return -1;
            }

            var headerTable = tableInfo.Keys.ToList();

            var sqlQuery = SqlLiteQueryConst.UpdateTable(tableAlias, headerTable, operators, where, value);

            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteUpdateQuery(sqlQuery, lst);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorUpdateTable, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);

                return false;
            }

            var sqlQuery = SqlLiteQueryConst.CreateUniqueIndex(tableAlias, column, indexName);

            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableKeyConstraint, Level = 0};
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
            var sqlQuery = SqlLiteQueryConst.DropUniqueIndex(indexName);

            if (sqlQuery != SqLiteHelperResources.ErrorCheck)
            {
                return ExecuteNonQuery(sqlQuery, SqLiteHelperResources.DoNotSuppressError);
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableKeyConstraint, Level = 0};
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
            return new DictionaryTableColumns {DColumns = InternalPragmaTableInfo(tableAlias)};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);

                return uniqueColumnsList;
            }

            var sqlQuery = SqlLiteQueryConst.Pragma_index_list(tableAlias);
            var table = SelectDataTable(sqlQuery);

            if (table == null)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableInfoNotFound, Level = 0};
                OnError(_message);

                return null;
            }

            var tableInfo = SqlLiteProcessing.CheckUniqueTableHeaders(table);

            if (tableInfo.Count == 0)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorCheckUniqueTableHeaders, Level = 1};
                OnError(_message);

                return new List<string>();
            }

            //Pragma_index_info
            foreach (var indexName in tableInfo)
            {
                sqlQuery = SqlLiteQueryConst.Pragma_index_info(indexName);
                table = SelectDataTable(sqlQuery);
                var columnName = SqlLiteProcessing.GetTableHeader(table);

                if (string.IsNullOrEmpty(columnName))
                {
                    _message = new MessageItem {Message = SqLiteHelperResources.ErrorGetTableHeader, Level = 0};
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
            var sqlQuery = SqlLiteQueryConst.GetTables();

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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
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
            if (sqlQuery == SqLiteHelperResources.ErrorCheck)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorSimpleSelectParameters, Level = 0};
                OnError(_message);

                return null;
            }

            var table = SelectMultipleDataTable(sqlQuery);
            if (table != null)
            {
                return table;
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorSimpleSelectExecution, Level = 0};
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
                using var conn = new SQLiteConnection(SqlLiteConnectionConfig.ConnectionString);
                conn.Open(); // throws if invalid
            }
            catch (SQLiteException ex1)
            {
                _message = new MessageItem {Message = ex1.ToString(), Level = 0};
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorDatabaseAlreadyExists, Level = 0};
                OnError(_message);

                return false;
            }

            SqlLiteConnectionConfig.Location = location;
            SqlLiteConnectionConfig.DbName = dbName;
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
                cmd.CommandTimeout = SqlLiteConnectionConfig.TimeOut;
                cmd.CommandText = sqlQuery;
                cmd.ExecuteNonQuery();
                tr.Commit();

                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
                };
                OnError(_message);
            }
            //catch error
            catch (SQLiteException ex1)
            {
                tr.Rollback();

                _message = new MessageItem {Message = string.Concat(ex1, sqlQuery), Level = 0};
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
            DataTable dt = null;

            var cmd = new SQLiteCommand {CommandText = sqlQuery, CommandTimeout = SqlLiteConnectionConfig.TimeOut};

            using var conn = GetConn();
            if (conn == null)
            {
                return null;
            }

            var command = new SQLiteCommand(cmd.CommandText, conn);
            var da = new SQLiteDataAdapter(command);

            _message = new MessageItem
            {
                Message = string.Concat(SqLiteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
            };
            OnError(_message);

            try
            {
                dt = new DataTable();
                da.Fill(dt);
            }
            //Normal Sql Exceptions
            catch (SQLiteException ex1)
            {
                _message = new MessageItem {Message = string.Concat(ex1, sqlQuery), Level = 0};
                OnError(_message);
            }
            //check if something went wrong when we selected something
            catch (NullReferenceException ex1)
            {
                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.ErrorInSelectStatement, sqlQuery), Level = 1
                };
                OnError(_message);
                _message = new MessageItem {Message = ex1.ToString(), Level = 0};
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

            var tm = SqlLiteProcessing.ConvertToTableMultipleSet(dt);

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
        private bool ExecuteInsertQuery(string sqlQuery, IReadOnlyCollection<TableSet> table,
            Dictionary<string, TableColumns> tableInfo, bool checking)
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
                cmd.CommandTimeout = SqlLiteConnectionConfig.TimeOut;

                //for checks only

                var checkCount = -1;

                //loop into all parameters
                foreach (var row in table)
                {
                    if (checking)
                    {
                        checkCount++;

                        var convert = tableInfo.ElementAt(checkCount).Value;
                        var check = AdvancedSyntaxCheck(tableInfo, table, row, convert);
                        if (!check)
                        {
                            return false;
                        }

                        /*
                         * Reset after max Count, missed that one.
                         * I did not reset it, which works fine if rows are equal or smaller than columns but that was a massive fuck up on my part.
                         */
                        if (checkCount == tableInfo.Count - 1)
                        {
                            checkCount = -1;
                        }
                    }

                    cmd.CommandText = sqlQuery;

                    for (var i = 0; i < row.Row.Count; i++)
                    {
                        var cell = row.Row[i];
                        _ = cmd.Parameters.Add(new SQLiteParameter(string.Concat(SqLiteHelperResources.Param, i),
                            cell));
                    }

                    cmd.ExecuteNonQuery();
                }

                tr.Commit();

                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
                };
                OnError(_message);
            }
            //catch error
            catch (SQLiteException ex1)
            {
                tr.Rollback();

                _message = new MessageItem {Message = string.Concat(ex1, sqlQuery), Level = 0};
                OnError(_message);

                return false;
            }

            return true;
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
            var lines = -1;

            //establish Connection
            using var conn = GetConn();
            if (conn == null)
            {
                return -1;
            }

            //begin Transaction
            using var tr = conn.BeginTransaction();
            //Try to execute our stuff
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tr;
                cmd.CommandTimeout = SqlLiteConnectionConfig.TimeOut;

                cmd.CommandText = sqlQuery;

                //loop into all parameters
                for (var i = 0; i < lst.Count; i++)
                {
                    var row = lst[i];
                    _ = cmd.Parameters.Add(new SQLiteParameter(string.Concat(SqLiteHelperResources.Param, i), row));
                }

                lines = cmd.ExecuteNonQuery();

                tr.Commit();

                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
                };
                OnError(_message);
            }
            //catch error
            catch (SQLiteException ex1)
            {
                tr.Rollback();

                _message = new MessageItem {Message = string.Concat(ex1, sqlQuery), Level = 0};
                OnError(_message);

                return lines;
            }

            return lines;
        }

        /// <summary>
        ///     Delete Entry
        /// </summary>
        /// <param name="sqlQuery">Query Text</param>
        /// <returns>Count of rows</returns>
        private int ExecuteDeleteQuery(string sqlQuery)
        {
            var lines = -1;

            //establish Connection
            using var conn = GetConn();
            if (conn == null)
            {
                return -1;
            }

            //begin Transaction
            using var tr = conn.BeginTransaction();
            //Try to execute our stuff
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tr;
                cmd.CommandTimeout = SqlLiteConnectionConfig.TimeOut;

                cmd.CommandText = sqlQuery;

                lines = cmd.ExecuteNonQuery();

                tr.Commit();

                _message = new MessageItem
                {
                    Message = string.Concat(SqLiteHelperResources.SuccessExecutedLog, sqlQuery), Level = 2
                };
                OnError(_message);
            }
            //catch error
            catch (SQLiteException ex1)
            {
                tr.Rollback();

                _message = new MessageItem {Message = string.Concat(ex1, sqlQuery), Level = 0};
                OnError(_message);

                return lines;
            }

            return lines;
        }

        /// <summary>
        ///     Basic sanity checks
        /// </summary>
        /// <param name="tableInfo">Headers, Data Type and Constraints of the table</param>
        /// <param name="table">Table Collection</param>
        /// <param name="row">Table Collection</param>
        /// <param name="convert">Table Collection</param>
        /// <returns>true if no errors were found</returns>
        private bool AdvancedSyntaxCheck(ICollection tableInfo,
            IEnumerable<TableSet> table,
            TableSet row, TableColumns convert)
        {
            if (tableInfo.Count != table.First().Row.Count)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorMoreElementsToAddThanRows, Level = 0};
                OnError(_message);

                return false;
            }

            var count = 0;

            foreach (var rows in row.Row)
            {
                var uniqueList = new List<string> {rows};
                count++;

                //check if nullable
                if (rows == null && !convert.NotNull)
                {
                    _message = new MessageItem {Message = SqLiteHelperResources.ErrorNotNullAble, Level = 0};
                    OnError(_message);

                    return false;
                }

                //check if convert-able Type
                var check = SqlLiteProcessing.CheckConvert(convert.DataType, rows);

                if (!check)
                {
                    _message = new MessageItem
                    {
                        Message = string.Concat(SqLiteHelperResources.ErrorWrongType, rows,
                            SqLiteHelperResources.ConvertTo,
                            convert.DataType),
                        Level = 0
                    };
                    OnError(_message);

                    return false;
                }

                //check if Unique, etc ...
                if (row.Row.Count != count
                    || uniqueList.Distinct().Count() == uniqueList.Count
                    || (!convert.PrimaryKey && !convert.Unique)
                )
                {
                    continue;
                }

                _message = new MessageItem {Message = SqLiteHelperResources.ErrorNotUnique, Level = 0};
                OnError(_message);

                return false;
            }

            return true;
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
                    Message = SqLiteHelperResources.ErrorInsertCouldNotGetTableInfoError, Level = 0
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

            return SqlLiteQueryConst.SimpleSelect(headerTable, header, tableAlias, oderBy, where, operators,
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
                SqLiteHelperResources.Spacing,
                SqLiteHelperResources.SqlWhere,
                SqLiteHelperResources.Spacing,
                whereValue, SqLiteHelperResources.Spacing,
                SqLiteHelperResources.SqlIn,
                SqLiteHelperResources.BracketOpen);

            for (var i = 0; i < inClause.Count - 1; i++)
            {
                sqlQuery = string.Concat(sqlQuery, inClause[i], SqLiteHelperResources.Comma);
            }

            sqlQuery = string.Concat(sqlQuery, inClause[^1], SqLiteHelperResources.BracketClose);

            if (oderBy == null)
            {
                return sqlQuery;
            }

            return string.Concat(sqlQuery, SqLiteHelperResources.Spacing, SqLiteHelperResources.SqlOrderBy,
                SqLiteHelperResources.Spacing, oderBy);
        }

        /// <summary>
        ///     Converts Table results to query
        /// </summary>
        /// <param name="dt">Database Query results</param>
        /// <returns>String of DB Status</returns>
        private static string GetAllData(DataTable dt)
        {
            var str = string.Concat(SqLiteHelperResources.MessageInitiate, Environment.NewLine);

            foreach (DataRow row in dt.Rows)
            {
                for (var i = 0; i < row.ItemArray.Length; i++)
                {
                    str = string.Concat(str, SqLiteHelperResources.Spacing, row[i], Environment.NewLine);
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
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableDoesNotExist, Level = 0};
                OnError(_message);

                return null;
            }

            var sqlQuery = SqlLiteQueryConst.Pragma_TableInfo(tableAlias);

            var table = SelectDataTable(sqlQuery);
            if (table == null)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorTableInfoNotFound, Level = 0};
                OnError(_message);

                return null;
            }

            var tableInfo = SqlLiteProcessing.ConvertTableHeaders(table);
            if (tableInfo == null)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorConvertTableInfos, Level = 0};
                OnError(_message);

                return null;
            }

            //other Master Procedure
            var tableHeaders = Pragma_index_list(tableAlias);

            if (tableHeaders == null)
            {
                _message = new MessageItem {Message = SqLiteHelperResources.ErrorPragmaIndexList, Level = 0};
                OnError(_message);

                return null;
            }

            tableInfo = SqlLiteProcessing.AddUniqueStatus(tableInfo, tableHeaders);

            if (tableInfo != null)
            {
                return tableInfo;
            }

            _message = new MessageItem {Message = SqLiteHelperResources.ErrorAddUniqueStatus, Level = 0};
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

            SqlLiteConnectionConfig.Location = location;
            SqlLiteConnectionConfig.DbName = dbName;
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
                var connection = new SQLiteConnection(SqlLiteConnectionConfig.ConnectionString);
                connection.Open();
                return connection;
            }
            /*
            * we should fail loud with an Exception, would be cleaner,
            * but wrong Connections are not that rare.The User gets the messages and the Queries will have to handle it separately
            * So not the right way, but the way we handle it
            */

            catch (SQLiteException ex)
            {
                _message = new MessageItem
                {
                    Message = string.Concat(ex, SqlLiteConnectionConfig.ConnectionString), Level = 0
                };
                OnError(_message);
                return null;
            }
            catch (BadImageFormatException ex)
            {
                _message = new MessageItem
                {
                    Message = string.Concat(ex, SqlLiteConnectionConfig.ConnectionString), Level = 0
                };
                OnError(_message);
                return null;
            }
            catch (DllNotFoundException ex)
            {
                _message = new MessageItem
                {
                    Message = string.Concat(ex, SqlLiteConnectionConfig.ConnectionString), Level = 0
                };
                OnError(_message);
                return null;
            }
        }

        /// <summary>
        ///     Inform Subscribers about the News
        /// </summary>
        /// <param name="dbMessage">Message</param>
        private void OnError(MessageItem dbMessage)
        {
            setMessage?.Invoke(this, dbMessage);
        }
    }
}
