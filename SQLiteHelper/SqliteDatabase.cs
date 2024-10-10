/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteDatabase.cs
 * PURPOSE:     Various Read and Write Operations for SqlLite, packed into a light weight wrapper for easy to use SqlLite Integration
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global, it is not used yet
// ReSharper disable MemberCanBeInternal, these are public information and Methods, they should be visible to the outside

// TODO Add new Interface for attach detach and faster access and async
// https://stackoverflow.com/questions/53183370/c-sharp-how-to-start-an-async-method-without-await-its-complete

namespace SqliteHelper
{
    /// <inheritdoc cref="ISqliteDatabase" />
    /// <summary>
    ///     General access Point and Configuration
    ///     Sources: http://www.sqlitetutorial.net/sqlite-alter-table/
    ///     Bucket List:
    ///     - Right now we can't drop or rename Columns! We can only recreate them
    ///     - We could add Columns, but that's about it
    ///     - Copy from one table to another: $"CREATE TABLE `{CopyTable}` AS SELECT * FROM `{SourceTable}`";
    ///     - copy table from one Database to another
    ///     - Sources:  https://www.codeproject.com/Questions/1157116/Sqlite-copy-data-from-all-db-tables-to-db-tables
    ///     https://tableplus.io/blog/2018/07/sqlite-how-to-copy-table-to-another-database.html
    ///     - Full Text Search
    ///     - Sources:  http://www.sqlitetutorial.net/sqlite-full-text-search/
    /// </summary>
    public sealed class SqliteDatabase : ISqliteDatabase
    {
        /// <summary>
        ///     The Util (readonly). Value: new SqlLiteUtility().
        /// </summary>
        private readonly SqliteUtility _util = new();

        /// <summary>
        ///     Execute our Tasks
        /// </summary>
        private SqliteExecute _execute;

        /// <summary>
        ///     Initiate Database
        /// </summary>
        public SqliteDatabase()
        {
            Location = Directory.GetCurrentDirectory();
            DbName = SqliteHelperResources.StandardName;

            InitiateSystem();
        }

        /// <summary>
        ///     Initiate Database
        /// </summary>
        /// <param name="dbName">Name of the Database</param>
        public SqliteDatabase(string dbName)
        {
            Location = Directory.GetCurrentDirectory();
            DbName = dbName;

            InitiateSystem();
        }

        /// <summary>
        ///     Initiate Database
        /// </summary>
        /// <param name="location">Path to location</param>
        /// <param name="dbName">Name of the Database</param>
        public SqliteDatabase(string location, string dbName)
        {
            Location = location;
            DbName = dbName;

            InitiateSystem();
        }

        /// <summary>
        ///     Initiate Database
        /// </summary>
        /// <param name="location">Path to location</param>
        /// <param name="dbName">Name of the Database</param>
        /// <param name="timeOut">Time Out</param>
        public SqliteDatabase(string location, string dbName, int timeOut)
        {
            Location = location;
            DbName = dbName;
            TimeOut = timeOut;

            InitiateSystem();
        }

        /// <summary>
        ///     Send the Message to the Outside World
        /// </summary>
        public EventHandler<string> SendMessage { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the Last Errors.
        /// </summary>
        public string LastErrors => MessageHandling.LastError;

        /// <inheritdoc />
        /// <summary>
        ///     Gets the List Errors.
        /// </summary>
        public List<string> ListErrors => MessageHandling.ListError;

        /// <inheritdoc />
        /// <summary>
        ///     Folder where the Database resides
        /// </summary>
        public string Location
        {
            get => SqliteConnectionConfig.Location;

            set => SqliteConnectionConfig.Location = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Name of the Database
        /// </summary>
        public string DbName
        {
            get => SqliteConnectionConfig.DbName;

            set => SqliteConnectionConfig.DbName = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Basic Value is 3
        /// </summary>
        public int DbVersion
        {
            get => SqliteConnectionConfig.DbVersion;

            set => SqliteConnectionConfig.DbVersion = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Basic Value is 3
        /// </summary>
        public int TimeOut
        {
            get => SqliteConnectionConfig.TimeOut;

            set => SqliteConnectionConfig.TimeOut = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns all Activities
        /// </summary>
        public List<string> LogFile => MessageHandling.LogFile;

        /// <inheritdoc />
        /// <summary>
        ///     Max Log size of Errors
        ///     Standard is 1000
        /// </summary>
        public int MaxLinesError
        {
            get => MessageHandling.MaxLinesError;
            set => MessageHandling.MaxLinesError = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Max Log size of Log Entries
        ///     Standard is 10000
        /// </summary>
        public int MaxLinesLog
        {
            get => MessageHandling.MaxLinesLog;
            set => MessageHandling.MaxLinesLog = value;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete a database file
        ///     Might need some checks
        /// </summary>
        /// <param name="overwrite">Overwrite existing Database</param>
        /// <returns>Operation Success</returns>
        public bool CreateDatabase(bool overwrite)
        {
            return _execute.CreateDatabase(Location, DbName, overwrite);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete a database file
        ///     Might need some checks
        /// </summary>
        /// <param name="dbName">Name of Database</param>
        /// <param name="overwrite">Overwrite existing Database</param>
        /// <returns>Operation Success</returns>
        public bool CreateDatabase(string dbName, bool overwrite)
        {
            DbName = dbName;
            return _execute.CreateDatabase(Location, dbName, overwrite);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete a database file
        ///     Might need some checks
        /// </summary>
        /// <param name="location">Local location of the Database</param>
        /// <param name="dbName">Name of Database</param>
        /// <param name="overwrite">Overwrite existing Database</param>
        /// <returns>Operation Success</returns>
        public bool CreateDatabase(string location, string dbName, bool overwrite)
        {
            Location = location;
            DbName = dbName;
            return _execute.CreateDatabase(Location, dbName, overwrite);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Makes a soft Copy of a Table and copies it to a new target Table
        ///     We could use:
        ///     var query = $"create Table {target} as select * from {location}";
        ///     but that does not copy the Index nor the Unique Status, so we use this workaround
        /// </summary>
        /// <param name="location">local Table</param>
        /// <param name="target">Target Table</param>
        /// <returns>Copies one Table to another</returns>
        public bool CopyTable(string location, string target)
        {
            var set = SimpleSelect(location);
            return CopyTable(location, target, set?.Row);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Makes a soft Copy of a Table and copies it to a new target Table
        ///     It does not copy the Index nor the Unique Status
        ///     Here we add just a specific subset of data we have to collect with various other Select Statements, Data Vector
        ///     should be compatible
        /// </summary>
        /// <param name="location">local Table</param>
        /// <param name="target">Target Table</param>
        /// <param name="tableHeaders">Specified sub segment of the data</param>
        /// <returns>Copies one Table to another</returns>
        public bool CopyTable(string location, string target, List<TableSet> tableHeaders)
        {
            var pragma = Pragma_TableInfo(location);
            var check = CreateTable(target, pragma);
            return tableHeaders == null ? check : InsertMultipleRow(target, tableHeaders, false);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete a database file
        /// </summary>
        /// <param name="location">Local location of the Database</param>
        /// <param name="dbName">Name of Database</param>
        /// <returns>Operation Success</returns>
        public bool DeleteDatabase(string location, string dbName)
        {
            return _execute.DeleteDatabase(location, dbName);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Rename a Table
        /// </summary>
        /// <param name="tblName">Name of the database</param>
        /// <param name="tblNameNew">New Name of the Database</param>
        /// <returns>Operation Success</returns>
        public bool RenameTable(string tblName, string tblNameNew)
        {
            return _execute.RenameTable(tblName, tblNameNew);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Attach Database
        ///     Use current Location and Database
        ///     Only active as long as the Connection is active
        ///     Absolutely useless right now in this state, I have to reevaluate the whole thing
        /// </summary>
        /// <param name="dbName">Name of the Database</param>
        /// <param name="alias">New Name of the Database</param>
        /// <returns>Operation Success</returns>
        public bool AttachDatabase(string dbName, string alias)
        {
            return _execute.AttachDatabase(dbName, alias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Throws  string with all Infos we can possible get about the Database
        /// </summary>
        /// <returns></returns>
        public string GetDatabaseInfos()
        {
            return _execute.GetDatabaseInfos();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Switch to a new database Context
        /// </summary>
        /// <param name="location">Local location to the Database</param>
        /// <param name="dbName">Name of the database</param>
        /// <returns>True if Connection is possible</returns>
        public bool DatabaseContextSwitch(string location, string dbName)
        {
            return _execute.DatabaseContextSwitch(location, dbName);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Checks if Table exists
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>Operation Success</returns>
        public bool CheckIfDatabaseTableExists(string tableAlias)
        {
            return _execute.CheckIfDatabaseTableExists(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Deletes Table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>Operation Success</returns>
        public bool DropTable(string tableAlias)
        {
            return _execute.DropTable(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Deletes Content of the table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>Operation Success</returns>
        public bool TruncateTable(string tableAlias)
        {
            return _execute.TruncateTable(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a Table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="tableHeaders">Name of the Table Headers</param>
        /// <returns>Operation Success</returns>
        public bool CreateTable(string tableAlias, DictionaryTableColumns tableHeaders)
        {
            return _execute.CreateTable(tableAlias, tableHeaders);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update Table
        ///     with generic object
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="where">identifier for where clause, always equal</param>
        /// <param name="value">string value</param>
        /// <param name="obj">Object we want to update</param>
        /// <returns>Count of rows updated</returns>
        public int UpdateTable(string tableAlias, CompareOperator operators, string where, string value, object obj)
        {
            var lst = _util.ConvertObjectToAttributes(obj);
            return _execute.UpdateTable(tableAlias, operators, where, value, lst);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update Table
        ///     With String list
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="where">identifier for where clause, always equal</param>
        /// <param name="value">string value</param>
        /// <param name="lst">List of values we want to update</param>
        /// <returns>Count of rows updated</returns>
        public int UpdateTable(string tableAlias, CompareOperator operators, string where, string value,
            List<string> lst)
        {
            return _execute.UpdateTable(tableAlias, operators, where, value, lst);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Adds a line into a Table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="row">Single Entry</param>
        /// <param name="checking">Shall the Insert be checked</param>
        /// <returns>Operation Success</returns>
        public bool InsertSingleRow(string tableAlias, TableSet row, bool checking)
        {
            var table = new List<TableSet> { row };
            return _execute.InsertMultipleRow(tableAlias, table, checking);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Delete Rows of a table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="where">Name of the column</param>
        /// <param name="value">Value</param>
        /// <returns>Rows deleted</returns>
        public int DeleteRows(string tableAlias, string where, string value)
        {
            return _execute.DeleteRows(tableAlias, where, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Adds multiple lines into a Table
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="rows">Multiple Entries</param>
        /// <param name="checking">Shall the Insert be checked</param>
        /// <returns>Operation Success</returns>
        public bool InsertMultipleRow(string tableAlias, List<TableSet> rows, bool checking)
        {
            return _execute.InsertMultipleRow(tableAlias, rows, checking);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create Unique Index on Column
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="column">Column Name</param>
        /// <param name="indexName">Name of the index</param>
        /// <returns>Operation Success</returns>
        public bool CreateUniqueIndex(string tableAlias, string column, string indexName)
        {
            return _execute.CreateUniqueIndex(tableAlias, column, indexName);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Drop Unique Index on Column
        /// </summary>
        /// <param name="indexName">Name of the Index</param>
        /// <returns>Operation Success</returns>
        public bool DropUniqueIndex(string indexName)
        {
            return _execute.DropUniqueIndex(indexName);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Collect all Data needed about a Table to add Elements
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>Dictionary with Name of the Column as Key and all Information about the Column as Value</returns>
        public DictionaryTableColumns Pragma_TableInfo(string tableAlias)
        {
            return _execute.PragmaTable_Info(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the Primary Columns
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>List of Columns that are PrimaryKey</returns>
        public List<string> Primary_Key_list(string tableAlias)
        {
            var dct = _execute.PragmaTable_Info(tableAlias);

            return (from item in dct.DColumns where item.Value.PrimaryKey select item.Key).ToList();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns a List with Names of all Table Headers with the Unique Property and that are not PrimaryKey
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <returns>List of Names of all Table Headers with the Unique Property</returns>
        [return: MaybeNull]
        public List<string> Pragma_index_list(string tableAlias)
        {
            return _execute.Pragma_index_list(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns a List of Table Names
        /// </summary>
        /// <returns>Name of all Tables</returns>
        public List<string> GetTables()
        {
            return _execute.GetTables();
        }

        /// <inheritdoc />
        /// <summary>
        ///     A simple basic select
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SimpleSelect(string tableAlias)
        {
            return _execute.SimpleSelect(tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     A simple basic select
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SimpleSelect(string tableAlias, List<string> headers)
        {
            return _execute.SimpleSelect(headers, tableAlias);
        }

        /// <inheritdoc />
        /// <summary>
        ///     A simple basic select with most options
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="where">Optional, where by table row</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="whereValue">Compare to</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SimpleSelect(string tableAlias, string where, CompareOperator operators, string whereValue)
        {
            return _execute.SimpleSelect(tableAlias, where, operators, whereValue);
        }

        /// <inheritdoc />
        /// <summary>
        ///     A simple basic select with most options
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="where">Optional, where by table row</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="whereValue">Compare to</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SimpleSelect(string tableAlias, List<string> headers,
            string where, CompareOperator operators, string whereValue)
        {
            return _execute.SimpleSelect(headers, tableAlias, where, operators, whereValue);
        }

        /// <inheritdoc />
        /// <summary>
        ///     A simple basic select with all options
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="where">Optional, where by table row</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="whereValue">Compare to</param>
        /// <param name="oderBy">Optional, order by header</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SimpleSelect(string tableAlias, List<string> headers,
            string where, CompareOperator operators, string whereValue, string oderBy)
        {
            return _execute.SimpleSelect(headers, tableAlias, oderBy, where, operators, whereValue);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Select all elements from the in Clause
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="whereValue">Optional, where by table row</param>
        /// <param name="inClause">All in specifics</param>
        /// <returns>v, results as string</returns>
        public DataSet SelectIn(string tableAlias, string whereValue, List<string> inClause)
        {
            return _execute.SelectIn(tableAlias, null, whereValue, inClause, null);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Select all elements from the in Clause
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="whereValue">Optional, where by table row</param>
        /// <param name="inClause">All in specifics</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SelectIn(string tableAlias, List<string> headers, string whereValue, List<string> inClause)
        {
            return _execute.SelectIn(tableAlias, headers, whereValue, inClause, null);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Select all elements from the in Clause
        /// </summary>
        /// <param name="tableAlias">Must Name of Table</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="whereValue">Optional, where by table row</param>
        /// <param name="inClause">All in specifics</param>
        /// <param name="oderBy">Optional, order by header</param>
        /// <returns>TableMultipleSet, results as string</returns>
        public DataSet SelectIn(string tableAlias, List<string> headers, string whereValue, List<string> inClause,
            string oderBy)
        {
            return _execute.SelectIn(tableAlias, headers, whereValue, inClause, oderBy);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Loads the CSV.
        ///     Database must be provided
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="tableHeaders">The table headers.</param>
        /// <param name="csv">The CSV.</param>
        /// <param name="headers">if set to <c>true</c> [headers].</param>
        /// <returns>
        ///     Loads the csv File into an existing Database
        /// </returns>
        public bool LoadCsv(string tableAlias, DictionaryTableColumns tableHeaders, List<List<string>> csv,
            bool headers)
        {
            if (csv == null || csv.Count == 0)
            {
                return false;
            }

            var check = _execute.CreateTable(tableAlias, tableHeaders);
            if (!check)
            {
                return false;
            }

            var table = SqliteHelper.LoadCsv(csv, headers);
            return _execute.InsertMultipleRow(tableAlias, table, true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Loads the CSV.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="csv">The CSV.</param>
        /// <param name="headers">if set to <c>true</c> [headers].</param>
        /// <returns>
        ///     Loads the csv File into an existing Database
        /// </returns>
        public bool LoadCsv(string tableAlias, List<List<string>> csv, bool headers)
        {
            if (csv == null || csv.Count == 0)
            {
                return false;
            }

            // If 'headers' is true, the first row of the CSV will be treated as headers
            var tableHeaders = new DictionaryTableColumns();

            if (headers)
            {
                var headerRow = csv[0]; // First row as headers
                foreach (var columnName in headerRow)
                {
                    tableHeaders.DColumns.Add(columnName, new TableColumns
                    {
                        DataType = SqLiteDataTypes.Text, // Assuming TEXT as default data type
                        Unique = false, // Default value
                        PrimaryKey = false, // Default value
                        NotNull = false // Default value
                    });
                }

                csv.RemoveAt(0); // Remove the header row from CSV data
            }
            else
            {
                // Handle case where headers are not included in the CSV (You may define default column names)
                for (var i = 0; i < csv[0].Count; i++)
                {
                    tableHeaders.DColumns.Add($"{SqliteHelperResources.ColumnName}{i + 1}", new TableColumns
                    {
                        DataType = SqLiteDataTypes.Text, // Default data type
                        Unique = false, // Default value
                        PrimaryKey = false, // Default value
                        NotNull = false // Default value
                    });
                }
            }

            // Create the table using the inferred or provided headers
            var check = _execute.CreateTable(tableAlias, tableHeaders);
            if (!check)
            {
                return false;
            }

            // Load the CSV data into a DataTable (or equivalent structure)
            var table = SqliteHelper.LoadCsv(csv,
                false); // Now 'headers' is false since we removed the first row if headers were used
            return _execute.InsertMultipleRow(tableAlias, table, true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Exports the CVS.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="headers">if set to <c>true</c> [headers].</param>
        /// <returns>
        ///     List of Lines, that should be converted into a csv
        /// </returns>
        [return: MaybeNull]
        public List<List<string>> ExportCvs(string tableAlias, bool headers)
        {
            var table = _execute.SimpleSelect(tableAlias);
            var info = _execute.PragmaTable_Info(tableAlias);

            if (headers)
            {
                return SqliteHelper.ExportCsv(table, info);
            }

            return SqliteHelper.ExportCsv(table);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Collect all System Data
        /// </summary>
        /// <returns>All Data of the current Connection and settings</returns>
        public Config GetConnectionDetails()
        {
            return new Config
            {
                DbName = DbName,
                Location = Location,
                TimeOut = TimeOut,
                DbVersion = DbVersion,
                LastError = LastErrors,
                ListErrors = ListErrors,
                MaxLinesError = MaxLinesError,
                MaxLinesLog = MaxLinesLog
            };
        }

        /// <summary>
        ///     Initiate Execute and Message Sub System
        /// </summary>
        private void InitiateSystem()
        {
            _execute = new SqliteExecute();
            _execute.setMessage += SetMessage;
            MessageHandling.ClearErrors();
        }

        /// <summary>
        ///     Send the News Out
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void SetMessage(object sender, MessageItem e)
        {
            var message = MessageHandling.SetMessage(e.Message, e.Level);
            SendMessage?.Invoke(this, message);
        }
    }

    /// <summary>
    ///     Simple Container that holds System Messages
    /// </summary>
    internal sealed class MessageItem
    {
        /// <summary>
        ///     Gets or sets the Error level.
        /// </summary>
        internal int Level { init; get; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        internal string Message { init; get; }
    }

    /// <summary>
    ///     Collection of generic Connection Infos
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        ///     Gets or sets the location.
        /// </summary>
        public string Location { get; internal init; }

        /// <summary>
        ///     Gets or sets the Database name.
        /// </summary>
        public string DbName { get; internal init; }

        /// <summary>
        ///     Gets or sets the time out.
        /// </summary>
        public int TimeOut { get; internal init; }

        /// <summary>
        ///     Gets or sets the db version.
        /// </summary>
        public int DbVersion { get; internal init; }

        /// <summary>
        ///     Gets or sets the last Errors.
        /// </summary>
        public string LastError { get; internal init; }

        /// <summary>
        ///     Gets or sets the List Errors.
        /// </summary>
        public List<string> ListErrors { get; internal init; }

        /// <summary>
        ///     Gets or sets the max lines error.
        /// </summary>
        public int MaxLinesError { get; internal init; }

        /// <summary>
        ///     Gets or sets the max lines log.
        /// </summary>
        public int MaxLinesLog { get; internal init; }
    }
}
