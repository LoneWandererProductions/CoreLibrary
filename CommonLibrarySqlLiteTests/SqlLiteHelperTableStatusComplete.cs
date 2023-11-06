/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperTableStatusComplete.cs
 * PURPOSE:     Test Table Status
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteHelper;

// ReSharper disable CyclomaticComplexity

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper table status complete unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperTableStatusComplete
    {
        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private readonly SqlLiteDatabase _target = new();

        /// <summary>
        ///     Check Status of Table
        /// </summary>
        [TestMethod]
        public void TestTableStatus()
        {
            _target.SendMessage += SharedHelperClass.DebugPrints;
            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.PathDatabaseTableStatus);

            //Check if file was created
            _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbTableStatus, true);

            var tableHeaders = SharedHelperClass.CreateTableHeadersSimple();

            var check = _target.CreateTable("TestTableStatus", tableHeaders);
            //add values
            Assert.IsTrue(check, "Test passed Add Table " + _target.LastErrors);

            //Get values
            var cache = _target.Pragma_TableInfo("TestTableStatus");

            //test
            if (cache == null)
            {
                check = false;
            }

            Assert.IsTrue(check, "Empty Result set: " + _target.LastErrors);

            //test
            Assert.IsTrue(cache.DColumns.TryGetValue("First", out var vector),
                "Test passed Get Result set First " + _target.LastErrors);
            //test
            if (vector?.PrimaryKey == true)
            {
                check = false;
            }

            if (vector != null && vector.DataType != SqLiteDataTypes.Text)
            {
                check = false;
            }

            if (vector?.Unique == true)
            {
                check = false;
            }

            if (vector?.NotNull == true)
            {
                check = false;
            }

            Assert.IsTrue(check, "First result was wrong: " + _target.LastErrors);

            Assert.IsTrue(cache.DColumns.TryGetValue("Second", out vector),
                "Test passed Get Result set Second " + _target.LastErrors);
            //test
            if (vector?.PrimaryKey == false)
            {
                check = false;
            }

            if (vector != null && vector.DataType != SqLiteDataTypes.Integer)
            {
                check = false;
            }

            if (vector?.Unique == false)
            {
                check = false;
            }

            if (vector?.NotNull == true)
            {
                check = false;
            }

            Assert.IsTrue(check, "Second result was wrong: " + _target.LastErrors);

            Assert.IsTrue(cache.DColumns.TryGetValue("Third", out vector),
                "Test passed Get Result set Third " + _target.LastErrors);

            //test
            if (vector?.PrimaryKey == true)
            {
                check = false;
            }

            if (vector != null && vector.DataType != SqLiteDataTypes.Integer)
            {
                check = false;
            }

            if (vector?.Unique == false)
            {
                check = false;
            }

            if (vector?.NotNull == false)
            {
                check = false;
            }

            Assert.IsTrue(check, "Third result was wrong: " + _target.LastErrors);

            Assert.IsTrue(cache.DColumns.TryGetValue("Fourth", out vector),
                "Test passed Get Result set Fourth " + _target.LastErrors);

            //test
            if (vector?.PrimaryKey == true)
            {
                check = false;
            }

            if (vector != null && vector.DataType != SqLiteDataTypes.Integer)
            {
                check = false;
            }

            if (vector?.Unique == false)
            {
                check = false;
            }

            if (vector?.NotNull == false)
            {
                check = false;
            }

            Assert.IsTrue(check, "Fourth result was wrong: " + _target.LastErrors);
        }
    }
}
