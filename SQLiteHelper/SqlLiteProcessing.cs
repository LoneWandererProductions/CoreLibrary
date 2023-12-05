/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SqlLiteProcessing.cs
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
    internal static class SqlLiteProcessing
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
            var vector = new Dictionary<string, TableColumns>();

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
                    NotNull = row.ItemArray[3].ToString() == SqLiteHelperResources.TableContentsName,
                    RowId = row.ItemArray[0].ToString()
                };

                if (row.ItemArray[5].ToString() == SqLiteHelperResources.TableContentsName)
                {
                    column.PrimaryKey = true;
                    column.Unique = true;
                }
                else
                {
                    column.PrimaryKey = false;
                }

                column = SetDataType(column, row.ItemArray[2].ToString());

                if (column == null)
                {
                    return null;
                }

                vector.Add(row.ItemArray[1].ToString(), column);
            }

            return vector;
        }

        /// <summary>
        ///     Looks up the Indexes for the called table
        ///     1 Name we only need that one
        /// </summary>
        /// <param name="table">Table of all Index Information</param>
        /// <returns>List of the Names with unique Columns</returns>
        internal static List<string> CheckUniqueTableHeaders(DataTable table)
        {
            return (from DataRow row in table.Rows select row.ItemArray[1]?.ToString()).ToList();
        }

        /// <summary>
        ///     Input is the Name of the Index
        ///     2 Name of Header
        /// </summary>
        /// <param name="table">Table with the Index Information</param>
        /// <returns>Name of the Column</returns>
        internal static string GetTableHeader(DataTable table)
        {
            return (from DataRow row in table.Rows select row.ItemArray[2]?.ToString()).FirstOrDefault();
        }

        /// <summary>
        ///     Sets Status of Unique
        /// </summary>
        /// <param name="tableInfo">Dictionary of Table Information</param>
        /// <param name="tableHeader">List of Unique Columns</param>
        /// <returns>Dictionary of Table Information with Unique checked</returns>
        internal static Dictionary<string, TableColumns> AddUniqueStatus(Dictionary<string, TableColumns> tableInfo,
            List<string> tableHeader)
        {
            foreach (var headers in tableInfo.SelectMany(info => tableHeader.Where(headers => info.Key == headers)))
            {
                tableInfo[headers].Unique = true;
            }

            return tableInfo;
        }

        /// <summary>
        ///     Checks if Source Type is convert-able to Target Type
        /// </summary>
        /// <param name="convert">Target Type as String</param>
        /// <param name="rows">Source Type as SqlLiteDataTypes</param>
        /// <returns>Convert-able true or false</returns>
        internal static bool CheckConvert(SqLiteDataTypes convert, string rows)
        {
            return convert switch
            {
                SqLiteDataTypes.DateTime => DateTime.TryParse(rows, out _),
                SqLiteDataTypes.Decimal => decimal.TryParse(rows, out _),
                SqLiteDataTypes.Integer => true,
                SqLiteDataTypes.Real => double.TryParse(rows, out _),
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
            var table = new DataSet();
            var rows = new List<TableSet>(dt.Rows.Count);

            foreach (DataRow row in dt.Rows)
            {
                var list = new TableSet();

                foreach (var vector in row.ItemArray.Select(item => item?.ToString()))
                {
                    list.Row.Add(vector);
                }

                rows.Add(list);
            }

            table.Row = rows;

            return table;
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
                case SqLiteHelperResources.SqlLiteDataTypeInteger:
                    column.DataType = SqLiteDataTypes.Integer;
                    return column;

                case SqLiteHelperResources.SqlLiteDataTypeDecimal:
                    column.DataType = SqLiteDataTypes.Decimal;
                    return column;

                case SqLiteHelperResources.SqlLiteDataTypeDateTime:
                    column.DataType = SqLiteDataTypes.DateTime;
                    return column;

                case SqLiteHelperResources.SqlLiteDataTypeReal:
                    column.DataType = SqLiteDataTypes.Real;
                    return column;

                case SqLiteHelperResources.SqlLiteDataTypeText:
                    column.DataType = SqLiteDataTypes.Text;
                    return column;

                default:
                    Trace.WriteLine(string.Concat(SqLiteHelperResources.TraceCouldNotConvert, type));
                    return null;
            }
        }
    }
}
