/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperTableStatusUnique.cs
 * PURPOSE:     Test Internals
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The SQL Lite internals unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteInternals
    {
        /// <summary>
        ///     The table one (const). Value: "First".
        /// </summary>
        private const string TableOne = "First";

        /// <summary>
        ///     The table two (const). Value: "Second".
        /// </summary>
        private const string TableTwo = "Second";

        /// <summary>
        ///     The table three (const). Value: "Third".
        /// </summary>
        private const string TableThree = "Third";

        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private static readonly SqliteDatabase Target = new();

        /// <summary>
        ///     Test if the system can handle basic database creation.
        /// </summary>
        [TestMethod]
        public void TestCreateDatabaseInternals()
        {
            Target.SendMessage += SharedHelperClass.DebugPrints;

            // Cleanup previous data
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreate);

            // Check if file was created
            Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDatabaseCreate, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreate),
                "Test failed to create database: " + Target.LastErrors);

            // Check if database is empty after creation
            var cache = Target.SimpleSelect(string.Empty);
            Assert.AreEqual(null, cache, $"Test failed, exception not caught: {Target.LastErrors}");
        }

        /// <summary>
        ///     Test if a database can be created and populated with multiple tables and rows.
        /// </summary>
        [TestMethod]
        public void TestCreateDatabaseBigInternals()
        {
            Target.SendMessage += SharedHelperClass.DebugPrints;

            // Cleanup previous complex data
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreateComplex);

            // Check if file was created
            Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbComplex, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreateComplex),
                "Test failed to create database: " + Target.LastErrors);

            // Create table headers and insert data into TableOne
            var tableHeaders = SharedHelperClass.CreateTableHeadersMultiple();
            var check = Target.CreateTable(TableOne, tableHeaders);
            Assert.IsTrue(check, TableOne + " Test failed to add table: " + Target.LastErrors);

            var table = SharedHelperClass.CreateContent();
            check = Target.InsertMultipleRow(TableOne, table, false);
            Assert.IsTrue(check, TableOne + " Test failed to insert rows into Table: " + Target.LastErrors);

            // Create table headers and insert data into TableTwo
            tableHeaders = SharedHelperClass.CreateTableHeadersMultiple();
            check = Target.CreateTable(TableTwo, tableHeaders);
            Assert.IsTrue(check, TableTwo + " Test failed to add table: " + Target.LastErrors);

            table = SharedHelperClass.CreateContent(); // Ensure CreateContent() is working correctly
            check = Target.InsertMultipleRow(TableTwo, table, false);
            Assert.IsTrue(check, TableTwo + " Test failed to insert rows into Table: " + Target.LastErrors);

            // Create table headers and insert data into TableThree
            tableHeaders = SharedHelperClass.CreateTableHeadersUniqueMultiple();
            check = Target.CreateTable(TableThree, tableHeaders);
            Assert.IsTrue(check, TableThree + " Test failed to add table: " + Target.LastErrors);

            table = SharedHelperClass.CreateAdvancedContent(); // Ensure CreateAdvancedContent() is fixed or mocked
            check = Target.InsertMultipleRow(TableThree, table, false);
            Assert.IsTrue(check, TableThree + " Test failed to insert rows into Table: " + Target.LastErrors);

            // Check the Pragma TableInfo for table structure
            var cache = Target.Pragma_TableInfo(TableThree);

            foreach (var item in cache.DColumns)
            {
                Trace.WriteLine(string.Concat(item.Key, " , ", item.Value.DataType, " , ", item.Value.Unique + " , ",
                    item.Value.PrimaryKey));
            }

            // Check constraints like Unique and PrimaryKey for specific columns
            Assert.AreEqual(false, cache.DColumns["First"].PrimaryKey,
                "First column should not have PrimaryKey: " + cache.DColumns["First"].PrimaryKey);
            Assert.AreEqual(true, cache.DColumns["First"].Unique,
                "First column should be Unique: " + cache.DColumns["First"].Unique);
            Assert.AreEqual(true, cache.DColumns["Second"].PrimaryKey,
                "Second column should have PrimaryKey: " + cache.DColumns["Second"].PrimaryKey);
            Assert.AreEqual(true, cache.DColumns["Second"].Unique,
                "Second column should be Unique: " + cache.DColumns["Second"].Unique);
            Assert.AreEqual(false, cache.DColumns["Third"].Unique,
                "Third column should not be Unique: " + cache.DColumns["Third"].Unique);
            Assert.AreEqual(true, cache.DColumns["Fourth"].Unique,
                "Fourth column should be Unique: " + cache.DColumns["Fourth"].Unique);

            // Check Pragma index list for the table
            var data = Target.Pragma_index_list(TableThree);

            if (data == null)
            {
                Assert.Fail("Pragma index list is empty");
            }

            foreach (var item in data)
            {
                Trace.WriteLine(item);
            }

            Assert.AreEqual(2, data.Count, TableThree + " Test failed for Unique Index: " + Target.LastErrors);
            Assert.AreEqual("Fourth", data[0], TableThree + " Test failed for Unique Index: " + data[0]);
            Assert.AreEqual("First", data[1], TableThree + " Test failed for Unique Index: " + data[1]);

            // Check Primary Key list for the table
            data = Target.Primary_Key_list(TableThree);

            Assert.AreEqual(1, data.Count, TableThree + " Test failed for PrimaryKey Index: " + Target.LastErrors);
            Assert.AreEqual("Second", data[0], TableThree + " Test failed for PrimaryKey: " + data[0]);
        }
    }
}
