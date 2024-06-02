/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/DataFormatterTests.cs
 * PURPOSE:     Some basic tests for DataFormatter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using System.Text;
using DataFormatter;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Test some special cases for the Data Formatter
    /// </summary>
    [TestClass]
    public class DataFormatterTests
    {
        /// <summary>
        ///     The test file path
        /// </summary>
        private const string TestFilePath = "testfile.txt";

        /// <summary>
        ///     Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            // Create a test file with UTF-8 encoding and special characters
            using var writer = new StreamWriter(TestFilePath, false, Encoding.UTF8);
            writer.WriteLine("Line 1 with Ü");
            writer.WriteLine("Line 2 with ö");
        }

        /// <summary>
        ///     Check if the whole cvs stuff works.
        /// </summary>
        [TestMethod]
        public void Cvs()
        {
            var lst = new List<List<string>>();

            for (var i = 0; i < 10; i++)
            {
                var line = new List<string>();

                for (var j = 0; j < 10; j++)
                {
                    line.Add(j.ToString());
                }

                lst.Add(line);
            }

            CsvHandler.WriteCsv(TestFilePath, lst);

            Assert.IsTrue(File.Exists(TestFilePath), "File exists");

            lst = CsvHandler.ReadCsv(TestFilePath, ',');

            Assert.AreEqual("0", lst[1][0], "Right Element");

            Assert.AreEqual("9", lst[2][9], "Right Element");

            FileHandleDelete.DeleteFile(TestFilePath);
        }

        /// <summary>
        ///     Tears down.
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            // Clean up the test file
            if (File.Exists(TestFilePath))
            {
                File.Delete(TestFilePath);
            }
        }

        /// <summary>
        ///     Reads the file should read special characters.
        /// </summary>
        [TestMethod]
        public void ReadFileShouldReadSpecialCharacters()
        {
            // Arrange
            var expectedLines = new List<string> { "Line 1 with Ü", "Line 2 with ö" };

            // Act
            var actualLines = ReadText.ReadFile(TestFilePath);

            // Assert
            CollectionAssert.AreEqual(expectedLines, actualLines);
        }
    }
}
