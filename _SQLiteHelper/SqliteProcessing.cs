/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SqliteProcessing.cs
 * PURPOSE:     Processing of various Database Objects processing and Checks
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SQLiteHelper
{
    /// <summary>
    ///     The sql lite processing class.
    /// </summary>
    internal static class SqliteProcessing
    {
        /// <summary>
        ///     Still missing if unique
        ///     Adds Type, not null, Primary Key
        ///     Collects all Attributes of the Column
        /// </summary>
        /// <param name="table">Table with infos of all Columns</param>
        /// <returns>Dictionary with Name of the Column as Key, and all further Information as Value</returns>
        [return: MaybeNull]
        internal static Dictionary<string, TableColumns> ConvertTableHeaders(DataTable table)
        {
            var headers = new Dictionary<string, TableColumns>();

            //0 cid not needed by us, id of column
            //1 name
            //2 type
            //3 not null is 1 is true
            //4 default Value
            //5 Primary Key 1 is primary Key
            foreach (DataRow row in table.Rows)
            {
                if (row == null) continue;

                var column = new TableColumns
                {
                    NotNull = row.ItemArray[3].ToString() == SqliteHelperResources.TableContentsName,
                    RowId = row.ItemArray[0].ToString(),
                    PrimaryKey = row.ItemArray[5].ToString() == SqliteHelperResources.TableContentsName,
                    Unique = row.ItemArray[5].ToString() == SqliteHelperResources.TableContentsName
                };

                column = SetDataType(column, row.ItemArray[2].ToString());

                if (column == null) return null;

                headers.Add(row.ItemArray[1].ToString(), column);
            }

            return headers;
        }

        /// <summary>
        ///     Looks up the Indexes for the called table
        ///     1 Name we only need that one
        /// </summary>
        /// <param name="table">Table of all Index Information</param>
        /// <returns>List of the Names with unique Columns</returns>
        internal static List<string> CheckUniqueTableHeaders(DataTable table)
        {
            return table.Rows.Cast<DataRow>()
                        .Select(row => row.ItemArray[1]?.ToString())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();
        }

        /// <summary>
        ///     Input is the Name of the Index
        ///     2 Name of Header
        /// </summary>
        /// <param name="table">Table with the Index Information</param>
        /// <returns>Name of the Column</returns>
        internal static string GetTableHeader(DataTable table)
        {
            return table.Rows.Cast<DataRow>()
                        .Select(row => row.ItemArray[2]?.ToString())
                        .FirstOrDefault();
        }

        /// <summary>
        ///     Sets Status of Unique
        /// </summary>
        /// <param name="tableInfo">Dictionary of Table Information</param>
        /// <param name="uniqueHeaders">List of Unique Columns</param>
        /// <returns>Dictionary of Table Information with Unique checked</returns>
        internal static Dictionary<string, TableColumns> AddUniqueStatus(Dictionary<string, TableColumns> tableInfo, List<string> uniqueHeaders)
        {
            foreach (var header in uniqueHeaders)
            {
                if (tableInfo.ContainsKey(header))
                {
                    tableInfo[header].Unique = true;
                }
            }

            return tableInfo;
        }

        /// <summary>
        ///     Checks if Source Type is convert-able to Target Type
        /// </summary>
        /// <param name="convert">Target Type as String</param>
        /// <param name="value">Source Type as SqlLiteDataTypes</param>
        /// <returns>Convert-able true or false</returns>
        internal static bool CheckConvert(SqLiteDataTypes convert, string value)
        {
            return convert switch
            {
                SqLiteDataTypes.DateTime => DateTime.TryParse(value, out _),
                SqLiteDataTypes.Decimal => decimal.TryParse(value, out _),
                SqLiteDataTypes.Integer => int.TryParse(value, out _),
                SqLiteDataTypes.Real => double.TryParse(value, out _),
                //it is a string so probably
                SqLiteDataTypes.Text => true,
                _ => false
            };
        }

        /// <summary>
        ///     Converts SqlLite Table in our own "preferred" format
        /// </summary>
        /// <param name="dt">Result Set as Table</param>
        /// <returns>Converted Table into String</returns>
        internal static DataSet ConvertToTableMultipleSet(DataTable dt)
        {
            return new DataSet
            {
                Row = dt.Rows.Cast<DataRow>()
                             .Select(row => new TableSet
                             {
                                 Row = row.ItemArray.Select(item => item?.ToString()).ToList()
                             })
                             .ToList()
            };
        }

        /// <summary>
        ///     Set DataType of TableColumns Object
        /// </summary>
        /// <param name="column">TableColumns Object</param>
        /// <param name="type">Expected Type</param>
        /// <returns>TableColumn with appreciated DataType</returns>
        [return: MaybeNull]
        private static TableColumns SetDataType(TableColumns column, string type)
        {
            switch (type.ToLower())
            {
                case SqliteHelperResources.SqlLiteDataTypeInteger:
                    column.DataType = SqLiteDataTypes.Integer;
                    return column;

                case SqliteHelperResources.SqlLiteDataTypeDecimal:
                    column.DataType = SqLiteDataTypes.Decimal;
                    return column;

                case SqliteHelperResources.SqlLiteDataTypeDateTime:
                    column.DataType = SqLiteDataTypes.DateTime;
                    return column;

                case SqliteHelperResources.SqlLiteDataTypeReal:
                    column.DataType = SqLiteDataTypes.Real;
                    return column;

                case SqliteHelperResources.SqlLiteDataTypeText:
                    column.DataType = SqLiteDataTypes.Text;
                    return column;

                default:
                    Trace.WriteLine($"{SqliteHelperResources.TraceCouldNotConvert}{type}");
                    return null;
            }
        }
    }
}
