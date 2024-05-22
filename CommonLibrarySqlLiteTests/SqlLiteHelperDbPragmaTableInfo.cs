/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperDbPragmaTableInfo.cs
 * PURPOSE:     Pragma Function Tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper db pragma table info unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperDbPragmaTableInfo
    {
        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqliteDatabase _target = new();

        /// <summary>
        ///     Test if we can Select a Database
        /// </summary>
        [TestMethod]
        public void TestDatabasePragmaTableInfo()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathPragmaTableInfo);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbPragmaIndexList, true);
            Assert.IsTrue(File.Exists(ResourcesSqlLite.PathPragmaTableInfo),
                "Test passed Create " + _target.LastErrors);

            var header = SharedHelperClass.CreateTableHeadersMultiple();
            //create the Table
            var check = _target.CreateTable("newOne", header);
            Assert.IsTrue(check, "Test failed Create Table" + _target.LastErrors);

            var cache = _target.Pragma_TableInfo("newOne");

            check = SharedHelperClass.CheckPragmaTableInfo(cache, header);
            Assert.IsTrue(check, "Test failed Compare Results" + _target.LastErrors);
        }
    }
}
