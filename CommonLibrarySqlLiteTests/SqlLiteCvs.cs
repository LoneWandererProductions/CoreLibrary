/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteCvs.cs
 * PURPOSE:     Test Cvs Imports and Exports
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     Test Import and Export functions
    /// </summary>
    [TestClass]
    public class SqlLiteCvs
    {
        /// <summary>
        ///     The CSV table
        /// </summary>
        private const string CsvTable = "Csv";

        /// <summary>
        ///     The target (readonly). Value: new SqlLiteDatabase().
        /// </summary>
        private static readonly SqliteDatabase Target = new();

        /// <summary>
        ///     CSVs the import export.
        /// </summary>
        [TestMethod]
        public void CsvImportExport()
        {
            Target.SendMessage += SharedHelperClass.DebugPrints;

            //cleanup
            SharedHelperClass.CleanUp(ResourcesSqlLite.DbImportExport);

            //Check if file was created
            Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbImportExport, true);

            Assert.IsTrue(File.Exists(ResourcesSqlLite.DbImportExport),
                "Test failed Create: " + Target.LastErrors);

            var elementOne = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var elementTwo = new TableColumns
            {
                DataType = SqLiteDataTypes.Integer, PrimaryKey = false, Unique = false, NotNull = true
            };

            var columns = new DictionaryTableColumns();

            columns.DColumns.Add("One", elementOne);
            columns.DColumns.Add("Two", elementTwo);

            var lst = new List<List<string>>();
            var header = new List<string> { "One", "Two" };
            lst.Add(header);
            var line = new List<string> { "1", "2" };
            lst.Add(line);
            line = new List<string> { "2", "4" };
            lst.Add(line);
            line = new List<string> { "5", "7" };
            lst.Add(line);
            line = new List<string> { "1", "0" };
            lst.Add(line);
            line = new List<string> { "10", "13" };
            lst.Add(line);
            line = new List<string> { "11", "5" };
            lst.Add(line);

            var check = Target.LoadCsv(CsvTable, columns, lst, true);

            Assert.IsTrue(check, CsvTable + " Test not passed Insert into Table: " + Target.LastErrors);

            var data = Target.SimpleSelect(CsvTable);

            Trace.WriteLine("First:");
            foreach (var item in data.Row)
            {
                foreach (var element in item.Row)
                {
                    Trace.Write($"{element},");
                }

                Trace.WriteLine(Environment.NewLine);
            }

            var result = Target.ExportCvs(CsvTable, true);

            Assert.IsNotNull(result, "Failed to export");

            Trace.WriteLine("Second:");
            foreach (var item in result)
            {
                foreach (var element in item)
                {
                    Trace.Write($"{element},");
                }

                Trace.WriteLine(Environment.NewLine);
            }

            Assert.AreEqual("One", result[0][0], "Right Element");
            Assert.AreEqual("0", result[4][1], "Right Element");
            Assert.AreEqual("5", result[6][1], "Right Element");
        }
    }
}
