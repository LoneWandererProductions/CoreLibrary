/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilderTests
 * FILE:        CoreBuilderTests/ResXtract.cs
 * PURPOSE:     Tests for ResXtract Tool.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.IO;
using System.Linq;
using CoreBuilder.Development;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreBuilderTests;

/// <summary>
///     test Class for ResXtract.
/// </summary>
[TestClass]
public class ResXtractTests
{
    /// <summary>
    ///     The resource xtract
    /// </summary>
    private ResXtract _resXtract;

    /// <summary>
    ///     Setups this instance.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        // Initialize the ResXtract tool with a possible ignore list and patterns if needed.
        _resXtract = new ResXtract();
    }

    /// <summary>
    ///     Extracts the strings from files should extract literal strings.
    /// </summary>
    [TestMethod]
    public void ExtractStringsFromFilesShouldExtractLiteralStrings()
    {
        // Arrange: Example source code (as a string) containing regular strings
        const string code =
            "var message = \"Hello, World!\";\n" +
            "var errorMessage = \"Error: \" + ex.Message;";

        // Act: Extract strings
        var extractedStrings = ResXtract.ExtractStrings(code);

        // Assert: Check if the expected strings are extracted
        Assert.IsTrue(extractedStrings.Contains("Hello, World!"));
        Assert.IsTrue(extractedStrings.Contains("Error: "));
    }

    /// <summary>
    ///     Extracts the strings from files should extract interpolated strings.
    /// </summary>
    [TestMethod]
    public void ExtractStringsFromFilesShouldExtractInterpolatedStrings()
    {
        // Arrange: Example source code containing an interpolated string
        const string code = @"
        var ex = new Exception(""Test"");
        var message = $""Error: {ex.Message} at {DateTime.Now}"";
    ";

        // Act: Extract strings
        var extractedStrings = ResXtract.ExtractStrings(code);

        foreach (var str in extractedStrings)
        {
            Trace.WriteLine($"Extracted: {str}");
        }

        // Assert: Check if the interpolated string was extracted correctly
        Assert.IsTrue(extractedStrings.Contains("Error: {0} at {1}"));
    }

    /// <summary>
    ///     Processes the project should extract strings and generate resource file.
    /// </summary>
    [TestMethod]
    public void ProcessProjectShouldExtractStringsAndGenerateResourceFile()
    {
        // Arrange: Set up a test project path and output file
        const string? testProjectPath = @"C:\Temp\TestProject"; // Adjust with the correct test path
        const string? outputResourceFile = @"C:\Temp\outputResourceNamespace.cs";

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
