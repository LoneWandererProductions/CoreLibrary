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
        ///     The test files path for encoding
        /// </summary>
        private static readonly string encodingFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Encoding");

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

            CsvHandler.WriteCsv(_testFilePath, lst);

            Assert.IsTrue(File.Exists(_testFilePath), "File exists");

            lst = CsvHandler.ReadCsv(_testFilePath, ',');

            Assert.AreEqual("0", lst[1][0], "Right Element");

            Assert.AreEqual("9", lst[2][9], "Right Element");

            FileHandleDelete.DeleteFile(_testFilePath);
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
        ///     Tests the read CSV.
        /// </summary>
        [TestMethod]
        public void TestReadCsv()
        {
            // Arrange
            const string csvContent = "123,Hello\n456,World";
            File.WriteAllText(_testFilePath, csvContent);
            const char separator = ',';

            // Act
            var result = CsvHandler.ReadCsv<MyCustomType>(_testFilePath, separator);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(123, result[0].Property1);
            Assert.AreEqual("Hello", result[0].Property2);

            Assert.AreEqual(456, result[1].Property1);
            Assert.AreEqual("World", result[1].Property2);
        }

        /// <summary>
        ///     Gets the file encoding ut f8 file returns ut f8 encoding.
        /// </summary>
        [TestMethod]
        public void GetFileEncodingUTF8FileReturnsUTF8Encoding()
        {
            var filePath = Path.Combine(encodingFilesPath, "utf8.txt");
            var encoding = DataHelper.GetFileEncoding(filePath);
            Assert.AreEqual(Encoding.UTF8.WebName, encoding.WebName);
        }

        /// <summary>
        ///     Gets the file encoding ut f8 file returns ansi8 encoding.
        /// </summary>
        [TestMethod]
        public void GetFileEncodingUTF8FileReturnsAnsi8Encoding()
        {
            var filePath = Path.Combine(encodingFilesPath, "ansi.txt");
            var encoding = DataHelper.GetFileEncoding(filePath);
            Assert.AreEqual(Encoding.Default, encoding);
        }

        /// <summary>
        /// Reads the CSV with layer keywords returns correct layers.
        /// </summary>
        [TestMethod]
        public void ReadCsvWithLayerKeywords_ReturnsCorrectLayers()
        {
            // Arrange
            var filepath = "test.csv";
            var separator = ',';
            var layerKeyword = "Layer_";

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
            var layers = CvsLayeredHandler.ReadCsvWithLayerKeywords(filepath, separator, layerKeyword);

            // Assert
            Assert.IsNotNull(layers, "The layers should not be null.");
            Assert.AreEqual(3, layers.Count, "There should be three layers.");

            // Check the content of the first layer
            var expectedFirstLayer = "Name,Age,Location\nAlice,30,Wonderland\nBob,25,Builderland";
            var actualFirstLayer = string.Join("\n", layers[0]);
            Assert.AreEqual(
                NormalizeLineEndings(expectedFirstLayer).TrimEnd(),
                NormalizeLineEndings(actualFirstLayer).TrimEnd(),
                "The content of the first layer is incorrect."
            );

            // Check the content of the second layer
            var expectedSecondLayer = "Name,Occupation\nCharlie,Engineer\nDana,Artist";
            var actualSecondLayer = string.Join("\n", layers[1]);
            Assert.AreEqual(
                NormalizeLineEndings(expectedSecondLayer).TrimEnd(),
                NormalizeLineEndings(actualSecondLayer).TrimEnd(),
                "The content of the second layer is incorrect."
            );

            // Check the content of the third layer
            var expectedThirdLayer = "Name,Score\nEve,95\nFrank,88";
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
        /// Normalizes the line endings.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Unify Newline stuff</returns>
        private string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n");
        }

    }

    /// <summary>
    ///     Test Class
    /// </summary>
    internal class MyCustomType
    {
        /// <summary>
        ///     Gets or sets the property1.
        /// </summary>
        /// <value>
        ///     The property1.
        /// </value>
        [CsvColumn(0, typeof(IntAttributeConverter))]
        public int Property1 { get; set; }

        /// <summary>
        ///     Gets or sets the property2.
        /// </summary>
        /// <value>
        ///     The property2.
        /// </value>
        [CsvColumn(1, typeof(StringAttributeConverter))]
        public string Property2 { get; set; }
    }
}
