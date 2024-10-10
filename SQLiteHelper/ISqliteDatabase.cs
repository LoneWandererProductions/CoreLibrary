/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/ISqliteDatabase.cs
 * PURPOSE:     SQLiteHelper Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global, it is a Library so not every Method has to be used
// ReSharper disable UnusedMethodReturnValue.Global

using System.Collections.Generic;

namespace SqliteHelper
{
    /// <summary>
    ///     The ISqlLiteDatabase interface.
    /// </summary>
    internal interface ISqliteDatabase
    {
        /// <summary>
        ///     Gets the last Errors.
        /// </summary>
        /// <value>The <see cref="string" />.</value>
        string LastErrors { get; }

        /// <summary>
        ///     Gets the list Errors.
        /// </summary>
        List<string> ListErrors { get; }

        /// <summary>
        ///     Gets the log file.
        /// </summary>
        List<string> LogFile { get; }

        /// <summary>
        ///     Gets or sets the location.
        /// </summary>
        /// <value>The <see cref="string" />.</value>
        string Location { get; set; }

        /// <summary>
        ///     Gets or sets the db name.
        /// </summary>
        /// <value>The <see cref="string" />.</value>
        string DbName { get; set; }

        /// <summary>
        ///     Gets or sets the db version.
        /// </summary>
        /// <value>The <see cref="int" />.</value>
        int DbVersion { get; set; }

        /// <summary>
        ///     Gets or sets the time out.
        /// </summary>
        /// <value>The <see cref="int" />.</value>
        int TimeOut { get; set; }

        /// <summary>
        ///     Gets or sets the max lines error.
        /// </summary>
        /// <value>The <see cref="int" />.</value>
        int MaxLinesError { set; get; }

        /// <summary>
        ///     Gets or sets the max lines log.
        /// </summary>
        /// <value>The <see cref="int" />.</value>
        int MaxLinesLog { set; get; }

        /// <summary>
        ///     The database context switch.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="dbName">The dbName.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool DatabaseContextSwitch(string location, string dbName);

        /// <summary>
        ///     Create the database.
        /// </summary>
        /// <param name="overwrite">The overwrite.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CreateDatabase(bool overwrite);

        /// <summary>
        ///     Create the database.
        /// </summary>
        /// <param name="dbName">The dbName.</param>
        /// <param name="overwrite">The overwrite.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CreateDatabase(string dbName, bool overwrite);

        /// <summary>
        ///     Create the database.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="dbName">The dbName.</param>
        /// <param name="overwrite">The overwrite.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CreateDatabase(string location, string dbName, bool overwrite);

        /// <summary>
        ///     Copy the table.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="target">The target.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CopyTable(string location, string target);

        /// <summary>
        ///     Copy the table.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="target">The target.</param>
        /// <param name="tableHeaders">The tableHeaders.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CopyTable(string location, string target, List<TableSet> tableHeaders);

        /// <summary>
        ///     Delete the database.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="dbName">The dbName.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool DeleteDatabase(string location, string dbName);

        /// <summary>
        ///     The rename table.
        /// </summary>
        /// <param name="tblName">The tblName.</param>
        /// <param name="tblNameNew">The tblNameNew.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool RenameTable(string tblName, string tblNameNew);

        /// <summary>
        ///     The attach database.
        /// </summary>
        /// <param name="dbName">The dbName.</param>
        /// <param name="alias">The alias.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool AttachDatabase(string dbName, string alias);

        /// <summary>
        ///     Get the database infos.
        /// </summary>
        /// <returns>The <see cref="string" />.</returns>
        string GetDatabaseInfos();

        /// <summary>
        ///     Check the if database table exists.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CheckIfDatabaseTableExists(string tableAlias);

        /// <summary>
        ///     The drop table.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool DropTable(string tableAlias);

        /// <summary>
        ///     The truncate table.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool TruncateTable(string tableAlias);

        /// <summary>
        ///     Create the table.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="tableHeaders">The tableHeaders.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CreateTable(string tableAlias, DictionaryTableColumns tableHeaders);

        /// <summary>
        ///     Update the table.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="operators">The operators.</param>
        /// <param name="where">The where.</param>
        /// <param name="value">The value.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="int" />.</returns>
        int UpdateTable(string tableAlias, CompareOperator operators, string where, string value, object obj);

        /// <summary>
        ///     Update the table.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="operators">The operators.</param>
        /// <param name="where">The where.</param>
        /// <param name="value">The value.</param>
        /// <param name="lst">The list.</param>
        /// <returns>The <see cref="int" />.</returns>
        int UpdateTable(string tableAlias, CompareOperator operators, string where, string value,
            List<string> lst);

        /// <summary>
        ///     Insert the single row.
        /// </summary>
        /// <param name="tableAlias">The Table Alias.</param>
        /// <param name="row">The row.</param>
        /// <param name="checking">The checking.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool InsertSingleRow(string tableAlias, TableSet row, bool checking);

        /// <summary>
        ///     Delete the rows.
        /// </summary>
        /// <param name="tableAlias">The Table Alias.</param>
        /// <param name="where">The where.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="int" />.</returns>
        int DeleteRows(string tableAlias, string where, string value);

        /// <summary>
        ///     Insert the multiple row.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="rows">The Table rows.</param>
        /// <param name="checking">The checking.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool InsertMultipleRow(string tableAlias, List<TableSet> rows, bool checking);

        /// <summary>
        ///     Create the unique index.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="column">The column.</param>
        /// <param name="indexName">The indexName.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool CreateUniqueIndex(string tableAlias, string column, string indexName);

        /// <summary>
        ///     The drop unique index.
        /// </summary>
        /// <param name="indexName">The indexName.</param>
        /// <returns>The <see cref="bool" />.</returns>
        bool DropUniqueIndex(string indexName);

        /// <summary>
        ///     The pragma table info.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="DictionaryTableColumns" />.</returns>
        DictionaryTableColumns Pragma_TableInfo(string tableAlias);

        /// <summary>
        ///     The primary key list.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="T:List{string}" />.</returns>
        List<string> Primary_Key_list(string tableAlias);

        /// <summary>
        ///     The pragma index list.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="T:List{string}" />.</returns>
        List<string> Pragma_index_list(string tableAlias);

        /// <summary>
        ///     Get the tables.
        /// </summary>
        /// <returns>The <see cref="T:List{string}" />.</returns>
        List<string> GetTables();

        /// <summary>
        ///     The simple select.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SimpleSelect(string tableAlias);

        /// <summary>
        ///     The simple select.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SimpleSelect(string tableAlias, List<string> headers);

        /// <summary>
        ///     The simple select.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="where">The where.</param>
        /// <param name="operators">The operators.</param>
        /// <param name="whereValue">The whereValue.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SimpleSelect(string tableAlias,
            string where, CompareOperator operators, string whereValue);

        /// <summary>
        ///     The simple select.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="where">The where.</param>
        /// <param name="operators">The operators.</param>
        /// <param name="whereValue">The whereValue.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SimpleSelect(string tableAlias, List<string> headers,
            string where, CompareOperator operators, string whereValue);

        /// <summary>
        ///     The simple select.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="where">The where.</param>
        /// <param name="operators">The operators.</param>
        /// <param name="whereValue">The where value.</param>
        /// <param name="oderBy">The oder by.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SimpleSelect(string tableAlias, List<string> headers,
            string where, CompareOperator operators, string whereValue, string oderBy);

        /// <summary>
        ///     Select the in.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="whereValue">The whereValue.</param>
        /// <param name="inClause">The inClause.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SelectIn(string tableAlias, string whereValue, List<string> inClause);

        /// <summary>
        ///     Select the in.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="whereValue">The whereValue.</param>
        /// <param name="inClause">The inClause.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SelectIn(string tableAlias, List<string> headers, string whereValue, List<string> inClause);

        /// <summary>
        ///     Select the in.
        /// </summary>
        /// <param name="tableAlias">The Table alias.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="whereValue">The where value.</param>
        /// <param name="inClause">The inClause.</param>
        /// <param name="oderBy">The oder by.</param>
        /// <returns>The <see cref="DataSet" />.</returns>
        DataSet SelectIn(string tableAlias, List<string> headers, string whereValue, List<string> inClause,
            string oderBy);

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
        bool LoadCsv(string tableAlias, DictionaryTableColumns tableHeaders, List<List<string>> csv, bool headers);

        /// <summary>
        ///     Loads the CSV.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="csv">The CSV.</param>
        /// <param name="headers">if set to <c>true</c> [headers].</param>
        /// <returns>
        ///     Loads the csv File into an existing Database
        /// </returns>
        bool LoadCsv(string tableAlias, List<List<string>> csv, bool headers);

        /// <summary>
        ///     Exports the CVS.
        /// </summary>
        /// <param name="tableAlias">The table alias.</param>
        /// <param name="headers">if set to <c>true</c> [headers].</param>
        /// <returns>
        ///     List of Lines, that should be converted into a csv
        /// </returns>
        List<List<string>> ExportCvs(string tableAlias, bool headers);

        /// <summary>
        ///     Get the connection details.
        /// </summary>
        /// <returns>The <see cref="Config" />.</returns>
        Config GetConnectionDetails();
    }
}
