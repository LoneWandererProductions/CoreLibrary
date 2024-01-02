/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SharedHelperClass.cs
 * PURPOSE:     Helper Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SQLiteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The shared helper class.
    /// </summary>
    internal static class SharedHelperClass
    {
        /// <summary>
        ///     Delete Database
        /// </summary>
        /// <param name="fullPath">Full Path</param>
        internal static void CleanUp(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        /// <summary>
        ///     Generate all table Headers
        /// </summary>
        /// <returns>Dictionary with Table Headers</returns>
        public static DictionaryTableColumns CreateTableHeadersSimple()
        {
            var elementOne = new TableColumns
            {
                DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
            };

            var elementTwo = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
            };

            var elementThree = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = true, NotNull = true
            };

            var columns = new DictionaryTableColumns();

            columns.DColumns.Add("First", elementOne);
            columns.DColumns.Add("Second", elementTwo);
            columns.DColumns.Add("Third", elementThree);
            columns.DColumns.Add("Fourth", elementThree);

            return columns;
        }

        /// <summary>
        ///     Generate all table Headers
        /// </summary>
        /// <returns>Dictionary with Table Headers</returns>
        public static DictionaryTableColumns CreateTableHeadersMultible()
        {
            var elementOne = new TableColumns
            {
                DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
            };

            var elementTwo = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = false
            };

            var elementThree = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var elementFour = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var elementFive = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var columns = new DictionaryTableColumns();

            columns.DColumns.Add("First", elementOne);
            columns.DColumns.Add("Second", elementTwo);
            columns.DColumns.Add("Third", elementThree);
            columns.DColumns.Add("Fourth", elementFour);
            columns.DColumns.Add("Five", elementFive);

            return columns;
        }

        /// <summary>
        ///     Like CreateTableHeadersMultible but with Unique Column
        /// </summary>
        /// <returns>Dictionary with Table Headers</returns>
        internal static DictionaryTableColumns CreateTableHeadersUniqueMultible()
        {
            var elementone = new TableColumns
            {
                DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = true, NotNull = false
            };

            var elementtwo = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
            };

            var elementthree = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var elementFour = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = true, NotNull = true
            };

            var elementfive = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var columns = new DictionaryTableColumns();

            columns.DColumns.Add("First", elementone);
            columns.DColumns.Add("Second", elementtwo);
            columns.DColumns.Add("Third", elementthree);
            columns.DColumns.Add("Fourth", elementFour);
            columns.DColumns.Add("Five", elementfive);

            return columns;
        }

        /// <summary>
        ///     Generate Content for Table
        /// </summary>
        /// <returns>Dictionary with Table Headers</returns>
        public static List<TableSet> CreateContent()
        {
            var columns = new List<TableSet>();
            for (var j = 0; j < 8; j++)
            {
                var tableSet = new TableSet();

                for (var i = 0; i < 5; i++)
                {
                    tableSet.Row.Add(i.ToString());
                }

                columns.Add(tableSet);
            }

            return columns;
        }

        /// <summary>
        ///     Generate Content for Table, numbered this time
        /// </summary>
        /// <returns>Dictionary with Table Headers</returns>
        internal static List<TableSet> CreateAdvancedContent()
        {
            var columns = new List<TableSet>();
            var count = -1;

            for (var j = 0; j < 8; j++)
            {
                var tableSet = new TableSet();

                for (var i = 0; i < 5; i++)
                {
                    count++;
                    tableSet.Row.Add(count.ToString());
                }

                columns.Add(tableSet);
            }

            return columns;
        }

        /// <summary>
        ///     Compares to Table Result Set
        /// </summary>
        /// <param name="tableone">Table One</param>
        /// <param name="tabletwo">Table Two</param>
        /// <returns>If Tables are equal or not</returns>
        internal static bool CompareTableMultibleSet(List<TableSet> tableone, List<TableSet> tabletwo)
        {
            if (tableone.Count != tabletwo.Count)
            {
                return false; // Different number of items
            }

            for (var i = 0; i < tableone.Count; i++)
            {
                var one = tableone[i];
                var two = tabletwo[i];
                if (!one.Row.SequenceEqual(two.Row))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Compares output and Input Data
        /// </summary>
        /// <param name="pragma">Data we get from the Pragma</param>
        /// <param name="headers">Data we sent</param>
        /// <returns>If Tables are equal or not</returns>
        internal static bool CheckPragmaTableInfo(DictionaryTableColumns pragma, DictionaryTableColumns headers)
        {
            if (pragma.DColumns.Count != headers.DColumns.Count)
            {
                return false; // Different number of items
            }

            foreach (var res in pragma.DColumns)
            {
                if (!headers.DColumns.TryGetValue(res.Key, out var head))
                {
                    return false; // key missing in b
                }

                if (!CompareValue(res.Value, head))
                {
                    return false; // value is different
                }
            }

            return true;
        }

        /// <summary>
        ///     Just Compare two Values
        /// </summary>
        /// <param name="res">Table one</param>
        /// <param name="head">Table two</param>
        /// <returns>If Tables are equal or not</returns>
        private static bool CompareValue(TableColumns res, TableColumns head)
        {
            if (res.DataType != head.DataType)
            {
                return false;
            }

            if (res.NotNull != head.NotNull)
            {
                return false;
            }

            if (res.PrimaryKey != head.PrimaryKey)
            {
                return false;
            }

            return res.Unique == head.Unique;
        }

        /// <summary>
        ///     Print out Debug Messages
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The error Message.</param>
        internal static void DebugPrints(object sender, string e)
        {
            Debug.WriteLine(e);
        }
    }
}
