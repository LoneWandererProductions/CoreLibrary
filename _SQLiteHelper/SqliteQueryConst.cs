/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SqliteQueryConst.cs
 * PURPOSE:     Const Sql Parameters for basic SqLite Operations, mostly basic Data types no Objects for portability, one Exception though I need a plan for this
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ArrangeBraces_for

using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteHelper
{
    /// <summary>
    ///     https://www.sqlite.org/pragma.html
    ///     Basic Constants for SqLite Queries
    /// </summary>
    internal static class SqliteQueryConst
    {
        /// <summary>
        ///     Helper to check for Primary Key
        /// </summary>
        private static bool _primaryKey;

        /// <summary>
        ///     Renames Table
        ///     https://www.sqlite.org/lang_altertable.html
        /// </summary>
        /// <param name="tableFrom">Table name old</param>
        /// <param name="tableTo">Table name new</param>
        /// <returns>Sql String</returns>
        internal static string RenameTable(string tableFrom, string tableTo)
        {
            return $"alter Table {tableFrom} rename to {tableTo}";
        }

        /// <summary>
        ///     Deletes Table
        ///     https://www.sqlite.org/lang_droptable.html
        /// </summary>
        /// <param name="tableAlias">Table name</param>
        /// <returns>Sql String</returns>
        internal static string DropTable(string tableAlias)
        {
            return $"Drop Table {tableAlias}";
        }

        /// <summary>
        ///     Truncates Table
        ///     https://www.sqlite.org/lang_delete.html
        /// </summary>
        /// <param name="tableAlias">Table name</param>
        /// <returns>Sql String</returns>
        internal static string TruncateTable(string tableAlias)
        {
            return $"Delete from {tableAlias}";
        }

        /// <summary>
        ///     Attach Database
        ///     https://www.sqlite.org/lang_attach.html
        ///     https://www.tutorialspoint.com/sqlite/sqlite_attach_database.htm
        /// </summary>
        /// <param name="database">Name of Database</param>
        /// <param name="alias">Name</param>
        /// <returns>Sql String</returns>
        internal static string AttachDatabaseTable(string database, string alias)
        {
            return $"attach DATABASE {database} as {alias}";
        }

        /// <summary>
        ///     Get overview of the Database
        ///     https://www.sqlite.org/fileformat.html
        /// </summary>
        /// <returns>Sql String</returns>
        internal static string GetDatabaseStatus()
        {
            return "select * from sqlite_master";
        }

        /// <summary>
        ///     Get overview of Table Simple Select
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <returns>Sql String</returns>
        internal static string SelectTable(string tableAlias)
        {
            return string.IsNullOrEmpty(tableAlias) ? SqliteHelperResources.ErrorCheck : $"select * from {tableAlias}";
        }

        /// <summary>
        ///     Creates a SqlLite Table with specific headers
        ///     https://www.sqlite.org/lang_createtable.html
        ///     https://www.tutorialspoint.com/sqlite/sqlite_create_table.htm
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <param name="tableHeaders">Headers of table</param>
        /// <returns>Sql String</returns>
        internal static string CreateTable(string tableAlias, DictionaryTableColumns tableHeaders)
        {
            if (string.IsNullOrEmpty(tableAlias))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            if (tableHeaders == null || tableHeaders.DColumns.Count == 0)
            {
                return SqliteHelperResources.ErrorCheck;
            }

            _primaryKey = false;
            return ConcatenateHeaders(tableHeaders, $"create TABLE {tableAlias}");
        }

        /// <summary>
        ///     Add Data into a SqlLite Table
        ///     https://www.tutorialspoint.com/sqlite/sqlite_insert_query.htm
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <param name="headerTable">Headers of table as String List</param>
        /// <returns>Sql String</returns>
        public static string InsertTable(string tableAlias, List<string> headerTable)
        {
            if (string.IsNullOrEmpty(tableAlias))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            if (headerTable == null || headerTable.Count == 0)
            {
                return SqliteHelperResources.ErrorCheck;
            }

            var queryStart = $"INSERT INTO {tableAlias} (";
            const string queryEnd = " VALUES(";

            return BuildConjunction(queryStart, queryEnd, headerTable);
        }

        /// <summary>
        ///     Remove data from SqlLite Table
        ///     https://www.tutorialspoint.com/sqlite/sqlite_insert_query.htm
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="where">Name of the column</param>
        /// <param name="value">Value</param>
        /// <returns>Sql String</returns>
        internal static string DeleteRows(string tableAlias, string where, string value)
        {
            if (string.IsNullOrEmpty(tableAlias) || string.IsNullOrEmpty(where) || string.IsNullOrEmpty(value))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            return $"DELETE from {tableAlias} WHERE {where} = '{value}'";
        }

        /// <summary>
        ///     Update Data in a SqlLite Table
        ///     https://www.tutorialspoint.com/sqlite/sqlite_update_query.htm
        ///     Example:
        ///     update new one set First = 1, Second=2, Third=3,Fourth=4, Five=5  where First = 1
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <param name="headerTable">Headers of table as String List</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="where">identifier for where clause, always equal</param>
        /// <param name="value">string value</param>
        /// <returns>Sql String</returns>
        internal static string UpdateTable(string tableAlias, List<string> headerTable, CompareOperator operators,
            string where, string value)
        {
            if (string.IsNullOrEmpty(tableAlias))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            if (headerTable == null || headerTable.Count == 0)
            {
                return SqliteHelperResources.ErrorCheck;
            }

            var queryStart = $"UPDATE {tableAlias} SET ";
            var queryEnd = string.Concat(
                SqliteHelperResources.Spacing, SqliteHelperResources.SqlWhere,
                SqliteHelperResources.Spacing, where, SqliteHelperResources.Spacing,
                OperatorToString(operators), SqliteHelperResources.Spacing,
                SqliteHelperResources.Escape, value, SqliteHelperResources.Escape
            );

            return BuildInsert(queryStart, queryEnd, headerTable);
        }

        /// <summary>
        ///     Pragma to get Table status
        ///     cid, name, type, notNull, dlft_value, PrimaryKey (0/1)
        ///     https://www.sqlite.org/pragma.html
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <returns>Sql String</returns>
        internal static string Pragma_TableInfo(string tableAlias)
        {
            return string.IsNullOrEmpty(tableAlias)
                ? SqliteHelperResources.ErrorCheck
                : $"PRAGMA table_info({tableAlias})";
        }

        /// <summary>
        ///     Pragma to get all Index_name
        ///     https://www.sqlite.org/pragma.html
        ///     https://www.tutorialspoint.com/sqlite/sqlite_pragma.htm
        /// </summary>
        /// <param name="tableAlias">name of table</param>
        /// <returns>Sql String</returns>
        internal static string Pragma_index_list(string tableAlias)
        {
            return string.IsNullOrEmpty(tableAlias)
                ? SqliteHelperResources.ErrorCheck
                : $"PRAGMA index_list({tableAlias})";
        }

        /// <summary>
        ///     Select all Table Names
        /// </summary>
        /// <returns>Sql String</returns>
        internal static string GetTables()
        {
            return "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
        }

        /// <summary>
        ///     Pragma to get Column Name of Index_name
        ///     https://www.sqlite.org/pragma.html
        ///     https://www.tutorialspoint.com/sqlite/sqlite_pragma.htm
        /// </summary>
        /// <param name="indexName">name of table</param>
        /// <returns>Sql String</returns>
        internal static string Pragma_index_info(string indexName)
        {
            return $"PRAGMA index_info({indexName})";
        }

        /// <summary>
        ///     A simple basic select
        ///     https://www.sqlite.org/lang_select.html
        /// </summary>
        /// <param name="headerTable">Name of the headers</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="tableAlias">Name of Table</param>
        /// <param name="oderBy">Optional, order by header</param>
        /// <param name="where">Optional, where by table row</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="whereValue">Compare to</param>
        /// <returns>Returns build Statement</returns>
        internal static string SimpleSelect(List<string> headerTable, List<string> headers,
            string tableAlias, string oderBy, string where, CompareOperator operators, string whereValue)
        {
            if (string.IsNullOrEmpty(tableAlias))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            if (headerTable == null || headerTable.Count == 0)
            {
                return SqliteHelperResources.ErrorCheck;
            }

            var queryStart = SqliteHelperResources.SqlSelect;
            string vector;

            if (headers != null)
            {
                vector = GetTableHeaders(queryStart, headers, headerTable);
                if (vector == SqliteHelperResources.ErrorCheck)
                {
                    return SqliteHelperResources.ErrorCheck;
                }
            }
            else
            {
                vector = SqliteHelperResources.SqlSelect + SqliteHelperResources.Star;
            }

            queryStart = vector;
            var queryEnd = $"from ({tableAlias})";

            // we got the basic select statement, now we only need the where clause
            if (headers != null)
            {
                vector = SetWhereClause(oderBy, where, operators, whereValue, headerTable);
                if (vector == SqliteHelperResources.ErrorCheck)
                {
                    return SqliteHelperResources.ErrorCheck;
                }

                queryEnd += vector;
            }
            else
            {
                if (operators == CompareOperator.None || string.IsNullOrEmpty(whereValue))
                {
                    return string.Concat(queryStart, queryEnd);
                }

                vector = string.Concat(
                    SqliteHelperResources.SqlWhere, SqliteHelperResources.Spacing, where,
                    SqliteHelperResources.Spacing, OperatorToString(operators),
                    SqliteHelperResources.Spacing, SqliteHelperResources.Escape, whereValue,
                    SqliteHelperResources.Escape
                );
                queryEnd += vector;
            }

            return string.Concat(queryStart, queryEnd);
        }

        /// <summary>
        ///     Create Unique Index on Column
        /// </summary>
        /// <param name="tableAlias">Name of the Table</param>
        /// <param name="column">Column Name</param>
        /// <param name="indexName">Name of the Index</param>
        /// <returns>SqlLite Query</returns>
        internal static string CreateUniqueIndex(string tableAlias, string column, string indexName)
        {
            if (string.IsNullOrEmpty(tableAlias) || string.IsNullOrEmpty(column) || string.IsNullOrEmpty(indexName))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            return $"CREATE UNIQUE INDEX {indexName} on {tableAlias}  ({column})";
        }

        /// <summary>
        ///     Drop Unique Index on Column
        /// </summary>
        /// <param name="indexName">Name of the Index</param>
        /// <returns>SqlLite Query</returns>
        internal static string DropUniqueIndex(string indexName)
        {
            return $"DROP INDEX {indexName}";
        }

        /// <summary>
        ///     Connects string for Create Table query
        /// </summary>
        /// <param name="tableHeaders">List of Names and Parameters</param>
        /// <param name="query">Start of the query</param>
        /// <returns>Table creation, empty if Key Constraint</returns>
        private static string ConcatenateHeaders(DictionaryTableColumns tableHeaders,
            string query)
        {
            var cache = new Dictionary<string, TableColumns>(tableHeaders.DColumns);

            var first = cache.First();
            var last = cache.Last();

            switch (cache.Count)
            {
                case 0:
                    return SqliteHelperResources.ErrorCheck;

                case 1:
                    var queryOne = query;
                    queryOne = string.Concat(queryOne, SqliteHelperResources.BracketOpen);
                    queryOne = AddRowDefinitions(queryOne, first);
                    if (queryOne == SqliteHelperResources.ErrorCheck)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    return string.Concat(queryOne, SqliteHelperResources.BracketClose);

                case 2:
                    var queryTwo = query;
                    queryTwo = string.Concat(queryTwo, SqliteHelperResources.BracketOpen);
                    queryTwo = AddRowDefinitions(queryTwo, first);
                    if (queryTwo == SqliteHelperResources.ErrorCheck)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    queryTwo = string.Concat(queryTwo, SqliteHelperResources.Comma);
                    queryTwo = AddRowDefinitions(queryTwo, last);
                    if (queryTwo == SqliteHelperResources.ErrorCheck)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    return string.Concat(queryTwo, SqliteHelperResources.BracketClose);

                default:
                    cache.Remove(first.Key);
                    cache.Remove(last.Key);

                    query = string.Concat(query, SqliteHelperResources.BracketOpen);
                    query = AddRowDefinitions(query, first);
                    query = string.Concat(query, SqliteHelperResources.Comma);
                    if (query == SqliteHelperResources.ErrorCheck)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    foreach (var headers in cache)
                    {
                        query = AddRowDefinitions(query, headers);
                        if (query == SqliteHelperResources.ErrorCheck)
                        {
                            return SqliteHelperResources.ErrorCheck;
                        }

                        query = string.Concat(query, SqliteHelperResources.Comma);
                    }

                    query = AddRowDefinitions(query, last);
                    return string.Concat(query, SqliteHelperResources.BracketClose);
            }
        }

        /// <summary>
        ///     Connects string for Update query
        ///     Example:
        ///     update newOne set First = 1, Second=2, Third=3,Fourth=4, Five=5  where First = 1
        /// </summary>
        /// <param name="queryStart">Start of the query</param>
        /// <param name="queryEnd">End of query</param>
        /// <param name="headerTable">List of Names and Parameters</param>
        /// <returns>Table creation, empty if Key Constraint</returns>
        private static string BuildInsert(string queryStart, string queryEnd, IReadOnlyList<string> headerTable)
        {
            var query = queryStart;

            for (var i = 0; i < headerTable.Count - 1; i++)
            {
                query = string.Concat(
                    query, headerTable[i],
                    SqliteHelperResources.CompEqual,
                    SqliteHelperResources.Param,
                    i,
                    SqliteHelperResources.Comma,
                    SqliteHelperResources.Spacing
                );
            }

            query = string.Concat(
                query,
                headerTable[^1],
                SqliteHelperResources.CompEqual,
                SqliteHelperResources.Param,
                headerTable.Count - 1,
                SqliteHelperResources.Spacing
            );

            return string.Concat(query, queryEnd);
        }

        /// <summary>
        ///     Add UNIQUE and PRIMARY KEY Parameters
        /// </summary>
        /// <param name="query">Current query text</param>
        /// <param name="value">Check Parameters</param>
        /// <returns>Added Parameters, empty if Key Constraint</returns>
        private static string AddRowDefinitions(string query, KeyValuePair<string, TableColumns> value)
        {
            if (value.Value.PrimaryKey)
            {
                if (_primaryKey)
                {
                    return SqliteHelperResources.ErrorCheck;
                }

                _primaryKey = true;
                query = string.Concat(query, $" {value.Key} ", $" {value.Value.DataType}  PRIMARY KEY ");
                if (value.Value.NotNull)
                {
                    query = string.Concat(query, SqliteHelperResources.SqlNotNull);
                }

                return query;
            }

            if (value.Value.Unique)
            {
                query = string.Concat(query, $" {value.Key} ", $" {value.Value.DataType}  UNIQUE ");
                if (value.Value.NotNull)
                {
                    query = string.Concat(query, SqliteHelperResources.SqlNotNull);
                }

                return query;
            }

            query = string.Concat(query, $" {value.Key} ", $" {value.Value.DataType}  ");
            if (value.Value.NotNull)
            {
                query = string.Concat(query, SqliteHelperResources.SqlNotNull);
            }

            return query;
        }

        /// <summary>
        ///     Dynamically build query Statement
        /// </summary>
        /// <param name="queryStart">Start of the query</param>
        /// <param name="queryEnd">End of the query</param>
        /// <param name="headerTable">Name of the table</param>
        /// <returns>Returns build Statement</returns>
        private static string BuildConjunction(string queryStart, string queryEnd, ICollection<string> headerTable)
        {
            var count = headerTable.Count;
            var endColumn = string.Concat(headerTable.Last(), SqliteHelperResources.BracketClose);
            //remove last element
            headerTable.Remove(headerTable.Last());

            var query = headerTable.Aggregate(queryStart,
                (current, key) => string.Concat(current, key, SqliteHelperResources.Comma));

            query = string.Concat(query, endColumn, queryEnd);

            for (var i = 0; i < count - 1; i++)
            {
                query = string.Concat(query, SqliteHelperResources.Param, i, SqliteHelperResources.Comma);
            }

            return string.Concat(query, SqliteHelperResources.Param, count - 1, SqliteHelperResources.BracketClose);
        }

        /// <summary>
        ///     Adds all Headers for the select Statement
        /// </summary>
        /// <param name="queryStart">Start of the query</param>
        /// <param name="headers">Optional, select over specific headers</param>
        /// <param name="headerTable">Headers of the table</param>
        /// <returns>Added Parameters, empty if wrong parameters</returns>
        private static string GetTableHeaders(string queryStart, IList<string> headers, IList<string> headerTable)
        {
            string lastRow;
            bool isFound;

            //custom headers
            switch (headers.Count)
            {
                case 0:
                    break;

                case 1:
                    isFound = headers.Intersect(headerTable).Any();
                    if (!isFound)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    lastRow = headers.Last();

                    return string.Concat(queryStart, lastRow, SqliteHelperResources.Spacing);

                default:
                    isFound = headers.Intersect(headerTable).Any();
                    if (!isFound)
                    {
                        return SqliteHelperResources.ErrorCheck;
                    }

                    lastRow = headers.Last();
                    headers.RemoveAt(headers.Count - 1);

                    queryStart = headers.Aggregate(queryStart,
                        (current, row) => string.Concat(current, row, SqliteHelperResources.Comma));

                    return string.Concat(queryStart, lastRow, SqliteHelperResources.Spacing);
            }

            //standard headers
            switch (headerTable.Count)
            {
                case 1:
                    lastRow = headerTable.Last();

                    return string.Concat(queryStart, lastRow, SqliteHelperResources.Spacing);

                default:
                    lastRow = headerTable.Last();

                    headerTable.RemoveAt(headerTable.Count - 1);

                    queryStart = headerTable.Aggregate(queryStart,
                        (current, row) => string.Concat(current, row, SqliteHelperResources.Comma));

                    return string.Concat(queryStart, lastRow, SqliteHelperResources.Spacing);
            }
        }

        /// <summary>
        ///     Generates the where Clause
        /// </summary>
        /// <param name="oderBy">Optional, order by header</param>
        /// <param name="where">Optional, where by table row</param>
        /// <param name="operators">Optional, if where is set essential, CompareOperator</param>
        /// <param name="whereValue">Compare to</param>
        /// <param name="headerTable">Headers of the table</param>
        /// <returns>Where Clause</returns>
        private static string SetWhereClause(string oderBy, string where, CompareOperator operators, string whereValue,
            List<string> headerTable)
        {
            var query = string.Empty;
            if (!string.IsNullOrEmpty(whereValue))
            {
                if (!headerTable.Exists(item => item == where))
                {
                    return SqliteHelperResources.ErrorCheck;
                }

                var comOperator = OperatorToString(operators);
                if (comOperator != SqliteHelperResources.CompNone)
                {
                    query = string.Concat(
                        SqliteHelperResources.Spacing, SqliteHelperResources.SqlWhere,
                        SqliteHelperResources.Spacing, where,
                        SqliteHelperResources.Spacing, OperatorToString(operators),
                        SqliteHelperResources.Spacing, SqliteHelperResources.Escape, whereValue,
                        SqliteHelperResources.Escape
                    );
                }
            }

            if (string.IsNullOrEmpty(oderBy))
            {
                return query;
            }

            if (!headerTable.Exists(item => item == oderBy))
            {
                return SqliteHelperResources.ErrorCheck;
            }

            return string.Concat(query, SqliteHelperResources.Spacing, SqliteHelperResources.SqlOrderBy,
                SqliteHelperResources.Spacing, oderBy);
        }

        /// <summary>
        ///     Converts Enum Operator to string
        /// </summary>
        /// <param name="operators">Enum of Operators</param>
        /// <returns>Compare Operator as string </returns>
        private static string OperatorToString(CompareOperator operators)
        {
            return operators switch
            {
                CompareOperator.Equal => SqliteHelperResources.CompEqual,
                CompareOperator.Like => SqliteHelperResources.CompLike,
                CompareOperator.NotLike => SqliteHelperResources.CompNotLike,
                CompareOperator.NotEqual => SqliteHelperResources.CompNotEqual,
                CompareOperator.None => SqliteHelperResources.CompNone,
                _ => throw new ArgumentOutOfRangeException(nameof(operators), operators, null)
            };
        }
    }
}
