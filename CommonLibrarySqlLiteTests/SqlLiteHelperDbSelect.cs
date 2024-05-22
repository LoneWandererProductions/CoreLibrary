/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperDbSelect.cs
 * PURPOSE:     Select tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper db select unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperDbSelect
    {
        /// <summary>
        ///     The Table name (const). Value: "First_Select".
        /// </summary>
        private const string TblName = "First_Select";

        /// <summary>
        ///     The Table name renamed (const). Value: "Second_Select".
        /// </summary>
        private const string TblNameRenamed = "Second_Select";

        /// <summary>
        ///     The First header (const). Value: "First".
        /// </summary>
        private const string FrstHeader = "First";

        /// <summary>
        ///     The Second header (const). Value: "Second".
        /// </summary>
        private const string ScdHeader = "Second";

        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqliteDatabase _target = new();

        /// <summary>
        ///     Test if we can Select a Database
        /// </summary>
        [TestMethod]
        public void DatabaseSelect()

        {
            _target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbSelect);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDatabaseSelect, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbSelect),
                "Test not passed Create: " + _target.LastErrors);

            var header = SharedHelperClass.CreateTableHeadersMultiple();
            //create the Table
            var check = _target.CreateTable(TblName, header);
            Assert.IsTrue(check, "Test not passed Create Table: " + _target.LastErrors);

            var table = SharedHelperClass.CreateContent();

            check = _target.InsertMultipleRow(TblName, table, false);
            Assert.IsTrue(check, "Test not passed Insert into Table: " + _target.LastErrors);

            //Select
            var cache = _target.SimpleSelect(TblName);

            var count = cache.Row.Count;
            Assert.AreEqual(8, count, "Test not passed Select Table" + _target.LastErrors);

            check = SharedHelperClass.CompareTableMultipleSet(table, cache.Row);

            Assert.IsTrue(check, "Test not passed Select Compare: " + _target.LastErrors);

            //Select simple
            cache = _target.SimpleSelect(TblName);

            Assert.AreEqual(false, cache.Raw == null, "Test not passed Dataview: " + _target.LastErrors);

            count = cache.Row.Count;
            Assert.AreEqual(8, count, "Test not passed Select Table: " + _target.LastErrors);

            check = SharedHelperClass.CompareTableMultipleSet(table, cache.Row);

            Assert.IsTrue(check, "Test not passed Select Compare: " + _target.LastErrors);

            //select only specific headers
            var lst = new List<string> { FrstHeader };
            cache = _target.SimpleSelect(TblName, lst);

            Assert.AreEqual(1, cache.Width,
                "Test not passed specific select, count: " + cache.Width + " possible errors: " +
                _target.LastErrors);

            //Select specific
            cache = _target.SimpleSelect(TblName, FrstHeader, CompareOperator.Equal, "0");

            Assert.AreEqual(8, cache.Row.Count,
                "Test not passed specific select, count: " + cache.Row.Count + " possible errors: " +
                _target.LastErrors);

            //Rename
            check = _target.RenameTable(TblName, TblNameRenamed);

            Assert.IsTrue(check, "Test not passed Rename Database: " + _target.LastErrors);

            //Truncate
            _target.TruncateTable(TblNameRenamed);

            cache = _target.SimpleSelect(TblNameRenamed);

            Assert.AreEqual(null, cache, "Test not passed Truncate Table: " + _target.LastErrors);

            //check if Table Exists
            Assert.AreEqual(true, _target.CheckIfDatabaseTableExists(TblNameRenamed),
                "Test passed Table exists: " + _target.LastErrors);
        }

        /// <summary>
        ///     Test if we can Select a Database, with more advanced features
        /// </summary>
        [TestMethod]
        public void AdvancedSelect()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbAdvancedSelect);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbAdvancedSelect, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.DbAdvancedSelect),
                "Test not passed Create: " + _target.LastErrors);

            var header = SharedHelperClass.CreateTableHeadersMultiple();

            //create the Table
            var check = _target.CreateTable(TblName, header);
            Assert.IsTrue(check, "Test not passed Create Table: " + _target.LastErrors);

            var table = SharedHelperClass.CreateAdvancedContent();

            check = _target.InsertMultipleRow(TblName, table, false);
            Assert.IsTrue(check, "Test not passed Insert into Table: " + _target.LastErrors);

            //Select simple
            var cache = _target.SimpleSelect(TblName);
            //check our custom Object

            Assert.AreEqual(8, cache.Height, "Test not passed Get Dimension Height: " + cache.Height);
            Assert.AreEqual(5, cache.Width, "Test not passed Get Dimension Width: " + cache.Width);

            var item = cache.Cell(7, 4);
            Assert.AreEqual("39", item, "Test not passed Get Value of Cell: " + item);

            //now for the real deal multiple where Clause
            var lst = new List<string> { FrstHeader };
            cache = _target.SimpleSelect(TblName, lst, FrstHeader, CompareOperator.Equal, "0");
            Assert.AreEqual(1, cache.Height, "Test not passed Get Dimension Height: " + cache.Height);
            Assert.AreEqual(1, cache.Width, "Test not passed Get Dimension Width: " + cache.Width);
            Assert.AreEqual("0", cache.Cell(0, 0), "Test not passed selected right Value: " + cache.Cell(0, 0));

            //more advanced
            lst.Add(ScdHeader);
            cache = _target.SimpleSelect(TblName, lst, FrstHeader, CompareOperator.Equal, "0", ScdHeader);
            Assert.AreEqual(1, cache.Height, "Test not passed Get Dimension Height: " + cache.Height);
            Assert.AreEqual(2, cache.Width, "Test not passed Get Dimension Width: " + cache.Width);
            Assert.AreEqual("0", cache.Cell(0, 0), "Test not passed selected right Value: " + cache.Cell(0, 0));
            Assert.AreEqual("1", cache.Cell(0, 1), "Test not passed selected right Value: " + cache.Cell(0, 1));
        }

        /// <summary>
        ///     Test if we can Select a Database, with Select In
        /// </summary>
        [TestMethod]
        public void SelectIn()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.DbSelectIn);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbSelectIn, true);

            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbDbSelectIn),
                "Test not passed Create: " + _target.LastErrors);

            var header = SharedHelperClass.CreateTableHeadersMultiple();

            //create the Table
            var check = _target.CreateTable(TblName, header);

            Assert.IsTrue(check, "Test not passed Create Table: " + _target.LastErrors);

            //data of the table
            var table = SharedHelperClass.CreateAdvancedContent();

            //insert the Data
            check = _target.InsertMultipleRow(TblName, table, false);
            Assert.IsTrue(check, "Test not passed Insert into Table: " + _target.LastErrors);

            //check our custom Object
            var lst = new List<string> { "0" };

            var headers = new List<string> { FrstHeader, ScdHeader };

            //test 1
            var cache = _target.SelectIn(TblName, FrstHeader, lst);

            Assert.AreEqual(1, cache.Row.Count, "Test not passed Right amount of Table contents: " + cache.Row.Count);

            //test 2
            lst.Add("5");
            cache = _target.SelectIn(TblName, headers, FrstHeader, lst);

            Assert.AreEqual(2, cache.Row.Count, "Test not passed Right amount of Table contents: " + cache.Row.Count);

            //test 3
            lst.Add("10");
            cache = _target.SelectIn(TblName, headers, FrstHeader, lst, FrstHeader);

            Assert.AreEqual(3, cache.Row.Count, "Test not passed Right amount of Table contents: " + cache.Row.Count);
        }
    }
}
