/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/DataFormatterTests.cs
 * PURPOSE:     Some basic tests for DataFormatter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using DataFormatter;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace CommonLibraryTests
{
    /// <summary>
    /// Test some special cases for the Data Formatter
    /// </summary>
    [TestClass]
    public class DataFormatterTests
    {
        /// <summary>
        /// The test file path
        /// </summary>
        private string testFilePath = "testfile.txt";

        /// <summary>
        /// Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            // Create a test file with UTF-8 encoding and special characters
            using var writer = new StreamWriter(testFilePath, false, System.Text.Encoding.UTF8);
            writer.WriteLine("Line 1 with Ü");
            writer.WriteLine("Line 2 with ö");
        }

        /// <summary>
        ///     Check if the whole cvs stuff works.
        /// </summary>
        [TestMethod]
        public void Cvs()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(DataFormatterTests),
                "cvsTest.cvs");

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

            CsvHandler.WriteCsv(path, lst);

            Assert.IsTrue(File.Exists(path), "File exists");

            lst = CsvHandler.ReadCsv(path, ',');

            Assert.AreEqual("0", lst[1][0], "Right Element");

            Assert.AreEqual("9", lst[2][9], "Right Element");

            FileHandleDelete.DeleteFile(path);
        }

        /// <summary>
        /// Tears down.
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            // Clean up the test file
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        /// <summary>
        /// Reads the file should read special characters.
        /// </summary>
        [TestMethod]
        public void ReadFileShouldReadSpecialCharacters()
        {
            // Arrange
            List<string> expectedLines = new List<string> { "Line 1 with Ü", "Line 2 with ö" };

            // Act
            List<string> actualLines = ReadText.ReadFile(testFilePath);

            // Assert
            CollectionAssert.AreEqual(expectedLines, actualLines);
        }
    }
}
