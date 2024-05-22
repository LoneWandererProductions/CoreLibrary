/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperDbUpdate.cs
 * PURPOSE:     Update tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;;

// ReSharper disable UnusedMember.Global
// ReSharper disable once MemberCanBeInternal

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper db update unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperDbUpdate
    {
        /// <summary>
        ///     The tbl name (const). Value: "newOne".
        /// </summary>
        private const string TblName = "newOne";

        /// <summary>
        ///     The frst header (const). Value: "First".
        /// </summary>
        private const string FirstHeader = "First";

        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqliteDatabase _target = new();

        /// <summary>
        ///     Test if we can create a Database
        /// </summary>
        [TestMethod]
        public void TestDatabaseUpdate()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;
            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbUpdate);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbUpdate, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbUpdate),
                "Test passed Create: " + _target.LastErrors);

            var header = SharedHelperClass.CreateTableHeadersMultiple();

            //create the Table
            var check = _target.CreateTable(TblName, header);
            Assert.IsTrue(check, "Test failed Create Table: " + _target.LastErrors);

            //insert
            var table = SharedHelperClass.CreateContent();
            check = _target.InsertMultipleRow(TblName, table, false);
            Assert.IsTrue(check, "Test failed Insert into Table: " + _target.LastErrors);

            var tableSet = new TestObject();

            var cache = _target.UpdateTable(TblName, CompareOperator.Equal, FirstHeader, "1", tableSet);

            //zero Rows for now
            Assert.AreEqual(0, cache, "Test failed Update Table: " + _target.LastErrors);

            cache = _target.UpdateTable(TblName, CompareOperator.Equal, FirstHeader, "0", tableSet);

            //zero Rows for now
            Assert.AreEqual(8, cache, "Test failed Update Table: " + cache);

            //with List this time
            _target.TruncateTable(TblName);
            check = _target.InsertMultipleRow(TblName, table, false);

            Assert.IsTrue(check, "Test failed Insert Table: " + cache);

            var lst = new List<string>
            {
                "1",
                "2",
                "3",
                "4",
                "5"
            };
            cache = _target.UpdateTable(TblName, CompareOperator.Equal, FirstHeader, "0", lst);
            Assert.AreEqual(8, cache, "Test failed Update Table: " + cache);
        }
    }

    /// <summary>
    ///     The test object class.
    /// </summary>
    public sealed class TestObject
    {
        public int One { get; set; } = 1;
        public int Two { get; set; } = 2;
        public int Three { get; set; } = 3;
        public int Four { get; set; } = 4;
        public int Five { get; set; } = 5;
    }
}
