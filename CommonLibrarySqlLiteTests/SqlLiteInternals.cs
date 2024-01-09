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
using SQLiteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite internals unit test class.
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
        private static readonly SqlLiteDatabase Target = new();

        /// <summary>
        ///     Test if our System can handle some crap
        /// </summary>
        [TestMethod]
        public void TestCreateDatabaseInternals()
        {
            Target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreate);

            //Check if file was created
            Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDatabaseCreate, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreate),
                "Test failed Create: " + Target.LastErrors);

            //check if file was deleted
            var cache = Target.SimpleSelect(string.Empty);

            Assert.AreEqual(null, cache, string.Concat("Test failed exception not caught: ", Target.LastErrors));
        }

        /// <summary>
        ///     Test if we can create a Database
        ///     And delete it afterwards
        /// </summary>
        [TestMethod]
        public void TestCreateDatabaseBigInternals()
        {
            Target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreate);

            //Check if file was created
            Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbComplex, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreateComplex),
                "Test failed Create: " + Target.LastErrors);

            var tableHeaders = SharedHelperClass.CreateTableHeadersMultiple();
            var check = Target.CreateTable(TableOne, tableHeaders);
            Assert.IsTrue(check, TableOne + " Test failed Add" + Target.LastErrors);

            var table = SharedHelperClass.CreateContent();
            check = Target.InsertMultipleRow(TableOne, table, false);
            Assert.IsTrue(check, TableOne + " Test not passed Insert into Table: " + Target.LastErrors);

            tableHeaders = SharedHelperClass.CreateTableHeadersMultiple();
            check = Target.CreateTable(TableTwo, tableHeaders);
            Assert.IsTrue(check, TableTwo + " Test failed Add" + Target.LastErrors);

            table = SharedHelperClass.CreateAdvancedContent();
            check = Target.InsertMultipleRow(TableTwo, table, false);
            Assert.IsTrue(check, TableTwo + " Test not passed Insert into Table: " + Target.LastErrors);

            tableHeaders = SharedHelperClass.CreateTableHeadersUniqueMultiple();
            check = Target.CreateTable(TableThree, tableHeaders);
            Assert.IsTrue(check, TableThree + " Test failed Add" + Target.LastErrors);

            table = SharedHelperClass.CreateAdvancedContent();
            check = Target.InsertMultipleRow(TableThree, table, false);
            Assert.IsTrue(check, TableThree + " Test not passed Insert into Table: " + Target.LastErrors);

            //Check Pragma I am a little bit paranoid
            var cache = Target.Pragma_TableInfo(TableThree);

            foreach (var item in cache.DColumns)
            {
                Debug.WriteLine(string.Concat(item.Key, " , ", item.Value.DataType, " , ", item.Value.Unique + " , ",
                    item.Value.PrimaryKey));
            }

            Assert.AreEqual(false, cache.DColumns["First"].PrimaryKey,
                "First Test not passed PrimaryKey: " + cache.DColumns["First"].PrimaryKey);
            Assert.AreEqual(true, cache.DColumns["First"].Unique,
                "First Test not passed Unique: " + cache.DColumns["First"].Unique);
            Assert.AreEqual(true, cache.DColumns["Second"].PrimaryKey,
                "Second Test not passed PrimaryKey" + cache.DColumns["Second"].PrimaryKey);
            Assert.AreEqual(true, cache.DColumns["Second"].Unique,
                "Second Test not passed Unique" + cache.DColumns["Second"].Unique);
            Assert.AreEqual(false, cache.DColumns["Third"].Unique,
                "Third Test not passed Unique" + cache.DColumns["Third"].Unique);
            Assert.AreEqual(true, cache.DColumns["Fourth"].Unique,
                "Fourth Test not passed Unique" + cache.DColumns["Fourth"].Unique);

            var data = Target.Pragma_index_list(TableThree);

            if (data == null)
            {
                Assert.Fail("Pragma was empty");
            }

            foreach (var item in data)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(2, data.Count, TableThree + " Test not passed Unique Index: " + Target.LastErrors);
            Assert.AreEqual("Fourth", data[0], TableThree + "First Test not passed Unique Index: " + data[0]);
            Assert.AreEqual("First", data[1], TableThree + "Fourth Test not passed Unique Index: " + data[1]);

            data = Target.Primary_Key_list(TableThree);

            Assert.AreEqual(1, data.Count, TableThree + " Test not passed PrimaryKey Index: " + Target.LastErrors);
            Assert.AreEqual("Second", data[0], TableThree + "First Test not passed PrimaryKey: " + data[0]);
        }
    }
}
