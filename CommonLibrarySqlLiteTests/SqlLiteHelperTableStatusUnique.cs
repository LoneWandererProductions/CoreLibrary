/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperTableStatusUnique.cs
 * PURPOSE:     Test Unique Table Status
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper table status unique unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperTableStatusUnique
    {
        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqlLiteDatabase _target = new();

        /// <summary>
        ///     Expand!
        ///     Check Status of Table
        /// </summary>
        [TestMethod]
        public void TestTableUniqueStatus()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;
            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbTableUniqueStatus);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbUniqueStatus, true);

            var tableHeaders = SharedHelperClass.CreateTableHeadersSimple();

            var check = _target.CreateTable("TableStatusTestUnique", tableHeaders);
            Assert.IsTrue(check, "Test failed Add" + _target.LastErrors);
            //add values
            var cache = _target.Pragma_index_list("TableStatusTestUnique");

            foreach (var element in cache)
            {
                Debug.WriteLine(element);
            }

            Assert.AreEqual(2, cache.Count, "Test failed Unique Count " + _target.LastErrors);
            Assert.IsTrue(cache.Contains("Third"), "Test failed Unique Third " + _target.LastErrors);
            Assert.IsTrue(cache.Contains("Fourth"), "Test failed Unique Fourth " + _target.LastErrors);
        }
    }
}
