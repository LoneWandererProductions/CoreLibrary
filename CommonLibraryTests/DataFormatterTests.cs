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
using System.Threading.Tasks;
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
        ///     The test files path for encoding
        /// </summary>
        private static readonly string EncodingFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Encoding");

        /// <summary>
        ///     The test file path
        /// </summary>
        private readonly string _testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "testFile.txt");

        /// <summary>
        ///     Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            // Create a test file with UTF-8 encoding and special characters
            using var writer = new StreamWriter(_testFilePath, false, Encoding.UTF8);
            writer.WriteLine("Line 1 with Ü");
            writer.WriteLine("Line 2 with ö");
        }

        /// <summary>
        ///     Check if the whole cvs stuff works.
        /// </summary>
        [TestMethod]
        public async Task CvsAsync()
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

            CsvHandler.WriteCsv(_testFilePath, lst);

            Assert.IsTrue(File.Exists(_testFilePath), "File exists");

            lst = CsvHandler.ReadCsv(_testFilePath, ',');

            Assert.AreEqual("0", lst[1][0], "Right Element");

            Assert.AreEqual("9", lst[2][9], "Right Element");

            _ = await FileHandleDelete.DeleteFile(_testFilePath);
        }

        /// <summary>
        ///     Tears down.
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            // Clean up the test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
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
            var actualLines = ReadText.ReadFile(_testFilePath);

            // Assert
            CollectionAssert.AreEqual(expectedLines, actualLines);
        }

        /// <summary>
        ///     Gets the file encoding ut f8 file returns ut f8 encoding.
        /// </summary>
        [TestMethod]
        public void GetFileEncodingUtf8FileReturnsUtf8Encoding()
        {
            var filePath = Path.Combine(EncodingFilesPath, "utf8.txt");
            var encoding = DataHelper.GetFileEncoding(filePath);
            Assert.AreEqual(Encoding.UTF8.WebName, encoding.WebName);
        }

        /// <summary>
        ///     Gets the file encoding ut f8 file returns ansi8 encoding.
        /// </summary>
        [TestMethod]
        public void GetFileEncodingUtf8FileReturnsAnsi8Encoding()
        {
            var filePath = Path.Combine(EncodingFilesPath, "ansi.txt");
            var encoding = DataHelper.GetFileEncoding(filePath);
            Assert.AreEqual(Encoding.Default, encoding);
        }

        /// <summary>
        ///     Writes the CSV with layer keywords creates expected output.
        /// </summary>
        [TestMethod]
        public void WriteCsvWithLayerKeywordsCreatesExpectedOutput()
        {
            // Arrange
            var tempPath = Path.GetTempFileName();
            var csvLayers = new List<List<string>> { new() { "1,2,3", "4,5,6" }, new() { "7,8,9" } };
            const char separator = ',';
            const string keyword = "#LAYER";

            // Act
            SegmentedCsvHandler.WriteCsvWithLayerKeywords(tempPath, separator, csvLayers, keyword);

            // Assert
            var expected = new[] { "1,2,3", "4,5,6", "#LAYER0", "7,8,9", "#LAYER1" };
            var actual = File.ReadAllLines(tempPath);
            CollectionAssert.AreEqual(expected, actual);

            File.Delete(tempPath); // Cleanup
        }

        /// <summary>
        ///     Reads the CSV with layer keywords returns correct layers.
        /// </summary>
        [TestMethod]
        public void ReadCsvWithLayerKeywordsReturnsCorrectLayers()
        {
            // Arrange
            const string filepath = "test.csv";
            const char separator = ',';
            const string layerKeyword = "Layer_";

            // Create test CSV content
            var csvContent = new List<string>
            {
                "Name,Age,Location",
                "Alice,30,Wonderland",
                "Bob,25,Builderland",
                "Layer_0",
                "Name,Occupation",
                "Charlie,Engineer",
                "Dana,Artist",
                "Layer_1",
                "Name,Score",
                "Eve,95",
                "Frank,88"
            };

            // Write the test content to the file
            File.WriteAllLines(filepath, csvContent);

            // Act
            var layers = SegmentedCsvHandler.ReadCsvWithLayerKeywords(filepath, separator, layerKeyword);

            // Assert
            Assert.IsNotNull(layers, "The layers should not be null.");
            Assert.AreEqual(3, layers.Count, "There should be three layers.");

            // Check the content of the first layer
            const string expectedFirstLayer = "Name,Age,Location\nAlice,30,Wonderland\nBob,25,Builderland";
            var actualFirstLayer = string.Join("\n", layers[0]);
            Assert.AreEqual(
                NormalizeLineEndings(expectedFirstLayer).TrimEnd(),
                NormalizeLineEndings(actualFirstLayer).TrimEnd(),
                "The content of the first layer is incorrect."
            );

            // Check the content of the second layer
            const string expectedSecondLayer = "Name,Occupation\nCharlie,Engineer\nDana,Artist";
            var actualSecondLayer = string.Join("\n", layers[1]);
            Assert.AreEqual(
                NormalizeLineEndings(expectedSecondLayer).TrimEnd(),
                NormalizeLineEndings(actualSecondLayer).TrimEnd(),
                "The content of the second layer is incorrect."
            );

            // Check the content of the third layer
            const string expectedThirdLayer = "Name,Score\nEve,95\nFrank,88";
            var actualThirdLayer = string.Join("\n", layers[2]);
            Assert.AreEqual(
                NormalizeLineEndings(expectedThirdLayer).TrimEnd(),
                NormalizeLineEndings(actualThirdLayer).TrimEnd(),
                "The content of the third layer is incorrect."
            );

            // Cleanup
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        /// <summary>
        ///     Normalizes the line endings.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Unify Newline stuff</returns>
        private static string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        /// <summary>
        ///     Test for reading a specific range of lines in the CSV file.
        /// </summary>
        [TestMethod]
        public void ReadCsvRangeReadsSpecifiedLinesOnly()
        {
            // Arrange
            const string filepath = "test_range.csv";
            const char separator = ',';
            var csvContent = new List<string> { "ID,Name", "1,Alice", "2,Bob", "3,Charlie" };
            File.WriteAllLines(filepath, csvContent);

            // Act
            var result = CsvHandler.ReadCsvRange(filepath, separator, parts => new { Id = parts[0], Name = parts[1] },
                1, 2);

            // Assert
            Assert.AreEqual(2, result.Count, "Should return two lines.");
            Assert.AreEqual("1", result[0].Id, "First ID should be '1'.");
            Assert.AreEqual("Alice", result[0].Name, "First name should be 'Alice'.");

            // Cleanup
            File.Delete(filepath);
        }

        /// <summary>
        ///     Test for writing CSV with a custom separator.
        /// </summary>
        [TestMethod]
        public void WriteCsvCustomSeparatorWritesCorrectly()
        {
            // Arrange
            const string filepath = "test_custom_separator.csv";
            const char customSeparator = ';';
            var lst = new List<List<string>> { new() { "ID", "Name" }, new() { "1", "Alice" }, new() { "2", "Bob" } };

            // Act
            CsvHandler.WriteCsv(filepath, lst, customSeparator.ToString());

            // Assert
            var lines = File.ReadAllLines(filepath);
            Assert.AreEqual("ID;Name", lines[0], "Header should be separated by semicolon.");
            Assert.AreEqual("1;Alice", lines[1], "Data should be separated by semicolon.");

            // Cleanup
            File.Delete(filepath);
        }
    }
}
