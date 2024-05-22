/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperUtility.cs
 * PURPOSE:     Test HelperUtility
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLiteHelper;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local just here for testing purposes

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The sql lite helper utility unit test class.
    /// </summary>
    [TestClass]
    public sealed class SqlLiteHelperUtility
    {
        /// <summary>
        ///     The Util (readonly). Value: new SqlLiteUtility().
        /// </summary>
        private readonly SqliteUtility _util = new();

        /// <summary>
        ///     Test the utility works
        /// </summary>
        [TestMethod]
        public void ConvertObject()
        {
            var tst = new TstObj();
            var cache = _util.ConvertObjectToTableColumns(tst);

            Assert.AreEqual(5, cache.DColumns.Count, "Wrong Number of Elements");

            var check = CheckResultsConvertObject(cache);

            Assert.IsTrue(check, "Test failed ConvertObject");
        }

        /// <summary>
        ///     Test the utility works, sadly has Problems with Enums
        /// </summary>
        [TestMethod]
        public void ConvertToTableRow()
        {
            var tst = new TstObjAttribute { First = "1", Second = "2", Third = 3, Fourth = 4 };

            var cache = _util.ConvertAttributes(tst);

            Assert.AreEqual(4, cache.Count, "Wrong Number of Elements");

            var check = CheckResultsConvertToTableRow(cache);

            Assert.IsTrue(check, "Test failed ConvertToAttribute");

            var data = _util.ConvertObjectToTableSet(tst);

            Assert.AreEqual(4, data.Row.Count, "Wrong Number of Elements");

            check = CheckResultsConvertToTableRow(data.Row);

            Assert.IsTrue(check, "Test failed ConvertToTableSet");
        }

        /// <summary>
        ///     Test the utility works, test for Enums
        /// </summary>
        [TestMethod]
        public void ConvertToTableRowEnums()
        {
            var tst = new TstObjEnumAttribute { Start = "1", End = TstEnm.Second };

            var cache = _util.ConvertObjectToAttributes(tst);

            Assert.AreEqual(2, cache.Count, "Wrong Number of Elements");
            Assert.IsTrue(cache[0] == "1", "Wrong Element Name: " + cache[0]);
            Assert.IsTrue(cache[1] == "Second", "Wrong Element Name: " + cache[1]);

            tst = new TstObjEnumAttribute { Start = "3", End = TstEnm.First };

            cache = _util.ConvertObjectToAttributes(tst);

            Assert.AreEqual(2, cache.Count, "Wrong Number of Elements");
            Assert.IsTrue(cache[0] == "3", "Wrong Element Name: " + cache[0]);
            Assert.IsTrue(cache[1] == "First", "Wrong Element Name: " + cache[1]);
        }

        /// <summary>
        ///     Test the utility works, in this case the mix between both Methods and the order of the results
        /// </summary>
        [TestMethod]
        public void ConvertToTableRowOrder()
        {
            var tst = new Order
            {
                First = "First",
                Alpha = "Alpha",
                Stout = "Stout",
                Fourth = "Fourth",
                HereWeGo = "HereWeGo"
            };

            var db = _util.ConvertObjectToTableColumns(tst);

            var data = _util.ConvertObjectToAttributes(tst);
            Assert.AreEqual(5, data.Count, "Wrong Number of Elements");
            data = _util.ConvertObjectToAttributes(tst);
            Assert.AreEqual(5, data.Count, "Wrong Number of Elements");

            var check = CheckResultsConvertToTableRowOrder(db.DColumns.Keys.ToList(), data);

            Assert.IsTrue(check, "Test failed ConvertToTableRowOrder");
        }

        /// <summary>
        ///     Test the utility works, in this case get the Table Data as Object, sadly has Problems with Enums
        /// </summary>
        [TestMethod]
        public void FillObject()
        {
            var tst = new Order
            {
                First = "First",
                Alpha = "Alpha",
                Stout = "Stout",
                Fourth = "Fourth",
                HereWeGo = "HereWeGo"
            };

            var data = _util.ConvertObjectToAttributes(tst);

            var ts = new Order();

            var db = (Order)_util.FillObjectFromAttributes(data, ts);

            Assert.IsTrue(tst.First == db.First, "Test failed First");
            Assert.IsTrue(tst.Alpha == db.Alpha, "Test failed Alpha");
            Assert.IsTrue(tst.Stout == db.Stout, "Test failed Stout");
            Assert.IsTrue(tst.Fourth == db.Fourth, "Test failed Fourth");
            Assert.IsTrue(tst.HereWeGo == db.HereWeGo, "Test failed HereWeGo");
        }

        /// <summary>
        ///     Test the utility works, in this case get the Table Data as Object, test for Enums
        /// </summary>
        [TestMethod]
        public void FillObjectEnum()
        {
            var tst = new TstObjEnumAttribute { Start = "1", End = TstEnm.Second };

            var data = _util.ConvertObjectToAttributes(tst);

            var ts = new TstObjEnumAttribute();

            var db = (TstObjEnumAttribute)_util.FillObjectFromAttributes(data, ts);

            Assert.IsTrue(tst.Start == db.Start, "Test failed First");
            Assert.IsTrue(tst.End == db.End, "Test failed Alpha");
        }

        /// <summary>
        ///     Test the utility works
        /// </summary>
        [TestMethod]
        public void DbCreation()
        {
            var path = Directory.GetCurrentDirectory() + @"\SqlLiteDB.db";
            //cleanup
            SharedHelperClass.CleanUp(path);

            var tst = new TstObj();
            var cache = _util.ConvertObjectToTableColumns(tst);

            Assert.AreEqual(5, cache.DColumns.Count, " Wrong Number of Elements");

            var check = CheckResultsConvertObject(cache);

            Assert.IsTrue(check, "Test failed wrong conversion");

            //basic initiation
            var db = new SqliteDatabase();

            //Check if file was created
            db.CreateDatabase(true);
            Assert.IsTrue(File.Exists(path), "Test not passes Create " + db.LastErrors);
            check = db.CreateTable("Helmet", cache);
            Assert.IsTrue(check, "Table not created " + db.LastErrors);

            check = db.CreateUniqueIndex("Helmet", "second", "id");
            Assert.IsTrue(check, "Index not created " + db.LastErrors);

            check = db.DropUniqueIndex("id");
            Assert.IsTrue(check, "Index deleted " + db.LastErrors);
            //cleanup
            SharedHelperClass.CleanUp(path);
        }

        /// <summary>
        ///     Just check if everything is correctly parsed
        /// </summary>
        /// <param name="lst">Result Set</param>
        /// <returns>Status</returns>
        private static bool CheckResultsConvertToTableRow(IReadOnlyList<string> lst)
        {
            Trace.WriteLine("0: " + lst[0]);
            Trace.WriteLine("1: " + lst[1]);
            Trace.WriteLine("2: " + lst[2]);
            Trace.WriteLine("3: " + lst[3]);

            if (lst[0] != "1")
            {
                return false;
            }

            if (lst[1] != "2")
            {
                return false;
            }

            if (lst[2] != "3")
            {
                return false;
            }

            return lst[3] == "4";
        }

        /// <summary>
        ///     Just check if everything is correctly parsed
        /// </summary>
        /// <param name="table">The Table</param>
        /// <returns>Status</returns>
        private static bool CheckResultsConvertObject(DictionaryTableColumns table)
        {
            if (table.DColumns["First"].DataType != SqLiteDataTypes.Text)
            {
                return false;
            }

            Trace.WriteLine("First");
            if (table.DColumns["Second"].DataType != SqLiteDataTypes.Integer)
            {
                return false;
            }

            Trace.WriteLine("Second");
            if (table.DColumns["Third"].DataType != SqLiteDataTypes.DateTime)
            {
                return false;
            }

            Trace.WriteLine("Third");
            if (table.DColumns["Fourth"].DataType != SqLiteDataTypes.Real)
            {
                return false;
            }

            Trace.WriteLine("Fourth");
            return table.DColumns["Fiveth"].DataType == SqLiteDataTypes.Decimal;
        }

        /// <summary>
        ///     Check the results convert to table row order.
        /// </summary>
        /// <param name="db">Data Table Info</param>
        /// <param name="data">Selected </param>
        /// <returns>The <see cref="bool" />.</returns>
        private static bool CheckResultsConvertToTableRowOrder(IReadOnlyList<string> db, IReadOnlyList<string> data)
        {
            Trace.WriteLine("0: " + db[0] + " " + data[0]);
            Trace.WriteLine("1: " + db[1] + " " + data[1]);
            Trace.WriteLine("2: " + db[2] + " " + data[2]);
            Trace.WriteLine("3: " + db[3] + " " + data[3]);
            Trace.WriteLine("4: " + db[4] + " " + data[4]);

            if (db[0] != data[0])
            {
                return false;
            }

            if (db[1] != data[1])
            {
                return false;
            }

            if (db[2] != data[2])
            {
                return false;
            }

            if (db[3] != data[3])
            {
                return false;
            }

            return db[4] == data[4];
        }

        /// <summary>
        ///     The test enum.
        /// </summary>
        private enum TstEnm
        {
            /// <summary>
            ///     The First = 0.
            /// </summary>
            First = 0,

            /// <summary>
            ///     The Second = 1.
            /// </summary>
            Second = 1
        }

        /// <summary>
        ///     The Test obj class.
        /// </summary>
        private sealed class TstObj
        {
            /// <summary>
            ///     Gets or sets the first.
            /// </summary>
            public string First { get; set; }

            /// <summary>
            ///     Gets or sets the second.
            /// </summary>
            public int Second { get; set; }

            /// <summary>
            ///     Gets or sets the third.
            /// </summary>
            public DateTime Third { get; set; }

            /// <summary>
            ///     Gets or sets the fourth.
            /// </summary>
            public float Fourth { get; set; }

            /// <summary>
            ///     Gets or sets the fiveth.
            /// </summary>
            public decimal Fiveth { get; set; }
        }

        /// <summary>
        ///     The Test obj attribute class.
        /// </summary>
        private sealed class TstObjAttribute
        {
            /// <summary>
            ///     Gets or sets the first.
            /// </summary>
            public string First { get; init; }

            /// <summary>
            ///     Gets or sets the second.
            /// </summary>
            public string Second { get; init; }

            /// <summary>
            ///     Gets or sets the third.
            /// </summary>
            public int Third { get; init; }

            /// <summary>
            ///     Gets or sets the fourth.
            /// </summary>
            public int Fourth { get; init; }
        }

        /// <summary>
        ///     The Test obj enum attribute class.
        /// </summary>
        private sealed class TstObjEnumAttribute
        {
            /// <summary>
            ///     Gets or sets the start.
            /// </summary>
            public string Start { get; init; }

            /// <summary>
            ///     Gets or sets the end.
            /// </summary>
            public TstEnm End { get; init; }
        }

        /// <summary>
        ///     The order class.
        /// </summary>
        private sealed class Order
        {
            /// <summary>
            ///     Gets or sets the first.
            /// </summary>
            public string First { get; init; }

            /// <summary>
            ///     Gets or sets the stout.
            /// </summary>
            public string Stout { get; init; }

            /// <summary>
            ///     Gets or sets the fourth.
            /// </summary>
            public string Fourth { get; init; }

            /// <summary>
            ///     Gets or sets the herewego.
            /// </summary>
            public string HereWeGo { get; init; }

            /// <summary>
            ///     Gets or sets the alpha.
            /// </summary>
            public string Alpha { get; init; }
        }
    }
}
