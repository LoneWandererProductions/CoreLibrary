/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperTableDelete.cs
 * PURPOSE:     Delete tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper table delete unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperTableDelete
    {
        /// <summary>
        ///     The table name (const). Value: "DeleteTest".
        /// </summary>
        private const string TableName = "DeleteTest";

        /// <summary>
        ///     The checking (const). Value: true.
        /// </summary>
        private const bool Checking = true;

        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqlLiteDatabase _target = new();

        /// <summary>
        ///     Test for creating Table
        ///     Create
        ///     Delete
        /// </summary>
        [TestMethod]
        public void TestDeleteTableRows()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;
            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbRowDelete);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDeleteRow, true);

            var elementone = new TableColumns
            {
                DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
            };

            var elementtwo = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
            };

            var columns = new DictionaryTableColumns();

            columns.DColumns.Add("First", elementone);
            columns.DColumns.Add("Second", elementtwo);

            var check = _target.CreateTable(TableName, columns);
            Assert.IsTrue(check, "Create Test did not pass " + _target.LastErrors);

            //First
            var lst = new List<string> {"2", "1"};
            var tableone = new TableSet {Row = lst};

            //Fill Data into Table
            check = _target.InsertSingleRow(TableName, tableone, Checking);

            Assert.IsTrue(check, "Insert first failed " + _target.LastErrors);

            //second
            lst = new List<string> {"4", "3"};
            tableone = new TableSet {Row = lst};

            //Fill Data into Table
            check = _target.InsertSingleRow(TableName, tableone, Checking);

            Assert.IsTrue(check, "Insert second failed " + _target.LastErrors);

            var count = _target.DeleteRows(TableName, "Second", "1");

            Assert.AreEqual(1, count, "Record not deleted" + _target.LastErrors);
        }
    }
}
