/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilderTests
 * FILE:        CoreBuilderTests/ResXtract.cs
 * PURPOSE:     Tests for ResXtract Tool.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CoreBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreBuilderTests
{
    /// <summary>
    /// test Class for ResXtract.
    /// </summary>
    [TestClass]
    public class ResXtractTests
    {
        /// <summary>
        /// The resource xtract
        /// </summary>
        private ResXtract _resXtract;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Initialize the ResXtract tool with a possible ignore list and patterns if needed.
            _resXtract = new ResXtract();
        }

        /// <summary>
        /// Extracts the strings from files should extract literal strings.
        /// </summary>
        [TestMethod]
        public void ExtractStringsFromFilesShouldExtractLiteralStrings()
        {
            // Arrange: Example source code (as a string) containing regular strings
            var code = @"
                var message = ""Hello, World!"";
                var errorMessage = ""Error: "" + ex.Message;
            ";

            // Act: Extract strings
            var extractedStrings = ResXtract.ExtractStrings(code);

            // Assert: Check if the expected strings are extracted
            Assert.IsTrue(extractedStrings.Contains("Hello, World!"));
            Assert.IsTrue(extractedStrings.Contains("Error: "));
        }

        /// <summary>
        /// Extracts the strings from files should extract interpolated strings.
        /// </summary>
        [TestMethod]
        public void ExtractStringsFromFilesShouldExtractInterpolatedStrings()
        {
            // Arrange: Example source code containing interpolated strings
            var code = @"
                var message = $""Error: {ex.Message} at {DateTime.Now}"";
            ";

            // Act: Extract strings
            var extractedStrings = ResXtract.ExtractStrings(code);

            // Assert: Check if the interpolated string was extracted correctly
            Assert.IsTrue(extractedStrings.Contains("Error: {0} at {1}"));
        }

        /// <summary>
        /// Generates the content of the resource file should create file with correct.
        /// </summary>
        [TestMethod]
        public void GenerateResourceFileShouldCreateFileWithCorrectContent()
        {
            // Arrange: Sample extracted strings (these would normally be extracted from actual code)
            var extractedStrings = new List<string>
            {
                "Hello, World!",
                "Error: {0} at {1}"
            };

            var outputFilePath = Path.Combine(Environment.CurrentDirectory, "TestResources.cs");

            // Act: Generate resource file (it will overwrite if the file exists)
            ResXtract.GenerateResourceFile(extractedStrings, outputFilePath, appendToExisting: false);

            // Assert: Check if the file exists and contains expected content
            Assert.IsTrue(File.Exists(outputFilePath));

            var fileContent = File.ReadAllLines(outputFilePath);

            // Check if the class is present in the file
            Assert.IsTrue(fileContent[0].Contains("public static class Resource"));

            // Check the specific resource definitions are present
            Assert.IsTrue(fileContent.Any(line => line.Contains("public static readonly string Resource1 = \"Hello, World!\";")));
            Assert.IsTrue(fileContent.Any(line => line.Contains("public static readonly string Resource2 = \"Error: {0} at {1}\";")));

            // Clean up
            File.Delete(outputFilePath);
        }

        /// <summary>
        /// Test if the ignore files based on pattern does work.
        /// </summary>
        [TestMethod]
        public void ShouldIgnoreFilesBasedOnPatterns()
        {
            // Arrange: Initialize ResXtract with an ignore pattern
            var ignorePatterns = new List<Regex> { new Regex(@"TestFile\.cs") };
            _resXtract = new ResXtract(ignorePatterns: ignorePatterns);

            // Act: Simulate file check
            var shouldIgnore = _resXtract.ShouldIgnoreFile("TestFile.cs");

            // Assert: Should ignore the file based on the pattern
            Assert.IsTrue(shouldIgnore);
        }

        /// <summary>
        /// Processes the project should extract strings and generate resource file.
        /// </summary>
        [TestMethod]
        public void ProcessProjectShouldExtractStringsAndGenerateResourceFile()
        {
            // Arrange: Set up a test project path and output file
            string testProjectPath = @"C:\Temp\TestProject"; // Adjust with the correct test path
            string outputResourceFile = @"C:\Temp\outputResourceNamespace.cs";

            // Prepare some test files in the project directory
            Directory.CreateDirectory(testProjectPath);
            string file1 = Path.Combine(testProjectPath, "TestFile1.cs");
            string file2 = Path.Combine(testProjectPath, "TestFile2.cs");
            File.WriteAllText(file1, "var message1 = \"Test1\";");
            File.WriteAllText(file2, "var message2 = \"Test2\";");

            // Act: Process the project to extract strings and generate the resource file
            _resXtract.ProcessProject(testProjectPath, outputResourceFile);

            // Assert: Check if the resource file was generated and contains the expected strings
            Assert.IsTrue(File.Exists(outputResourceFile));
            string[] generatedLines = File.ReadAllLines(outputResourceFile);
            Assert.IsTrue(generatedLines.Any(line => line.Contains("Test1")));
            Assert.IsTrue(generatedLines.Any(line => line.Contains("Test2")));

            // Clean up
            File.Delete(file1);
            File.Delete(file2);
            Directory.Delete(testProjectPath, true);
            File.Delete(outputResourceFile);
        }
    }
}
