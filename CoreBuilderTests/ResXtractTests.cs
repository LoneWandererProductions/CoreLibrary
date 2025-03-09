using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CoreBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreBuilderTests
{
    [TestClass]
    public class ResXtractTests
    {
        private ResXtract _resXtract;

        [TestInitialize]
        public void Setup()
        {
            // Initialize the ResXtract tool with a possible ignore list and patterns if needed.
            _resXtract = new ResXtract();
        }

        [TestMethod]
        public void ExtractStringsFromFiles_ShouldExtractLiteralStrings()
        {
            // Arrange: Example source code (as a string) containing regular strings
            var code = @"
                var message = ""Hello, World!"";
                var errorMessage = ""Error: "" + ex.Message;
            ";

            // Act: Extract strings
            var extractedStrings = _resXtract.ExtractStrings(code);

            // Assert: Check if the expected strings are extracted
            Assert.IsTrue(extractedStrings.Contains("Hello, World!"));
            Assert.IsTrue(extractedStrings.Contains("Error: "));
        }

        [TestMethod]
        public void ExtractStringsFromFiles_ShouldExtractInterpolatedStrings()
        {
            // Arrange: Example source code containing interpolated strings
            var code = @"
                var message = $""Error: {ex.Message} at {DateTime.Now}"";
            ";

            // Act: Extract strings
            var extractedStrings = _resXtract.ExtractStrings(code);

            // Assert: Check if the interpolated string was extracted correctly
            Assert.IsTrue(extractedStrings.Contains("Error: {0} at {1}"));
        }

        [TestMethod]
        public void GenerateResourceFile_ShouldCreateFileWithCorrectContent()
        {
            // Arrange: Sample extracted strings (these would normally be extracted from actual code)
            var extractedStrings = new List<string> { "Hello, World!", "Error: {0} at {1}" };

            var outputFilePath = Path.Combine(Environment.CurrentDirectory, "TestResources.cs");

            // Act: Generate resource file
            ResXtract.GenerateResourceFile(extractedStrings, outputFilePath);

            // Assert: Check if the file exists and contains expected content
            Assert.IsTrue(File.Exists(outputFilePath));

            var fileContent = File.ReadAllLines(outputFilePath);
            Assert.IsTrue(fileContent[0].Contains("public static readonly string Resource1 = \"Hello, World!\";"));
            Assert.IsTrue(fileContent[1].Contains("public static readonly string Resource2 = \"Error: {0} at {1}\";"));

            // Clean up
            File.Delete(outputFilePath);
        }

        [TestMethod]
        public void ShouldIgnoreFilesBasedOnPatterns()
        {
            // Arrange: Initialize ResXtract with an ignore pattern
            var ignorePatterns = new List<Regex> { new(@"TestFile\.cs") };
            _resXtract = new ResXtract(ignorePatterns: ignorePatterns);

            // Act: Simulate file check
            var shouldIgnore = _resXtract.ShouldIgnoreFile("TestFile.cs");

            // Assert: Should ignore the file based on the pattern
            Assert.IsTrue(shouldIgnore);
        }

        [TestMethod]
        public void ProcessProject_ShouldExtractStringsAndGenerateResourceFile()
        {
            // Arrange: Set up a test project path and output file
            var testProjectPath = @"C:\Temp\TestProject"; // Adjust with the correct test path
            var outputResourceFile = @"C:\Temp\outputResourceNamespace.cs";

            // Prepare some test files in the project directory
            Directory.CreateDirectory(testProjectPath);
            var file1 = Path.Combine(testProjectPath, "TestFile1.cs");
            var file2 = Path.Combine(testProjectPath, "TestFile2.cs");
            File.WriteAllText(file1, "var message1 = \"Test1\";");
            File.WriteAllText(file2, "var message2 = \"Test2\";");

            // Act: Process the project to extract strings and generate the resource file
            _resXtract.ProcessProject(testProjectPath, outputResourceFile);

            // Assert: Check if the resource file was generated and contains the expected strings
            Assert.IsTrue(File.Exists(outputResourceFile));
            var generatedLines = File.ReadAllLines(outputResourceFile);
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
