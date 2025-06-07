/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CoreBuilder
* FILE:        CoreBuilder/ResXtract.cs
* PURPOSE:     String Resource extractor.
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder
{
    /// <inheritdoc />
    /// <summary>
    ///     Our Code resource refactor tool. In this case strings.
    /// </summary>
    /// <seealso cref="T:CoreBuilder.IResourceExtractor" />
    public sealed class ResXtract : IResourceExtractor
    {
        /// <summary>
        ///     The ignore list
        /// </summary>
        private readonly List<string> _ignoreList;

        /// <summary>
        ///     The ignore patterns
        /// </summary>
        private readonly List<string> _ignorePatterns;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResXtract" /> class.
        /// </summary>
        /// <param name="ignoreList">The ignore list.</param>
        /// <param name="ignorePatterns">The ignore patterns.</param>
        public ResXtract(List<string> ignoreList = null, List<string> ignorePatterns = null)
        {
            _ignoreList = ignoreList ?? new List<string>();
            _ignorePatterns = ignorePatterns ?? new List<string>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Processes the given project directory, extracting string literals
        ///     and replacing them with resource references.
        /// </summary>
        /// <param name="projectPath">The root directory of the C# project.</param>
        /// <param name="outputResourceFile">Path to generate the resource file. If null, a default file will be used.</param>
        /// <param name="appendToExisting">If true, appends to the existing resource file, otherwise overwrites it.</param>
        /// <returns>List of changed Files with directory.</returns>
        public List<string> ProcessProject(string projectPath, string outputResourceFile = null,
            bool appendToExisting = false)
        {
            // If no output file is provided, set a default file path relative to the project path
            outputResourceFile ??= Path.Combine(projectPath, "ResourceFile.cs");

            var files = Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains("resource", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains("const", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains(@"\bin\", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains(@"\.vs\", StringComparison.OrdinalIgnoreCase));

            var extractedStrings = new List<string>();
            var changedFiles = new List<string>();

            foreach (var file in files)
            {
                if (ShouldIgnoreFile(file))
                {
                    continue;
                }

                var code = File.ReadAllText(file);
                var strings = ExtractStrings(code).ToList();

                if (strings.Any())
                {
                    extractedStrings.AddRange(strings);
                    changedFiles.Add(Path.GetFullPath(file)); // Store full path of changed file
                }
            }

            GenerateResourceFile(extractedStrings.Distinct().ToList(), outputResourceFile, appendToExisting);
            changedFiles.Add(Path.GetFullPath(outputResourceFile)); // Also include generated resource file

            return changedFiles;
        }

        /// <summary>
        ///     Determines whether the specified file should be ignored.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><c>true</c> if the file should be ignored; otherwise, <c>false</c>.</returns>
        private bool ShouldIgnoreFile(string filePath)
        {
            // Check if it's on the ignore list or matches known patterns
            if (_ignoreList.Contains(filePath) || _ignorePatterns.Any(pattern =>
                    filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return CoreHelper.ShouldIgnoreFile(filePath);
        }

        /// <summary>
        ///     Extracts the strings.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Extracted strings.</returns>
        public static IEnumerable<string> ExtractStrings(string code)
        {
            // Parse the code to a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            // List to hold the strings we find
            var stringLiterals = new List<string>();

            // Extract interpolated strings first, ensuring we extract the correct parts
            var interpolatedStrings = ExtractInterpolatedStrings(code);

            // Add regular string literals from the syntax tree
            stringLiterals.AddRange(root.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
                .Select(l => l.Token.ValueText));

            // Add interpolated string literals (only the extracted format string)
            foreach (var (_, extracted, _) in interpolatedStrings) // Discarding the `original` and `placeholders`
            {
                stringLiterals.Add(extracted); // Only adding the `extracted` part of the tuple
            }

            return stringLiterals;
        }


        /// <summary>
        ///     Extracts the interpolated strings.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>List of string that needs replacing</returns>
        private static IEnumerable<(string original, string extracted, List<string> placeholders)>
            ExtractInterpolatedStrings(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            var interpolatedStrings = new List<(string, string, List<string>)>();

            foreach (var node in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>())
            {
                var staticParts = new List<string>();
                var placeholders = new List<string>();

                var index = 0;
                foreach (var content in node.Contents)
                {
                    if (content is InterpolatedStringTextSyntax textPart)
                    {
                        staticParts.Add(textPart.TextToken.ValueText);
                    }
                    else if (content is InterpolationSyntax exprPart)
                    {
                        staticParts.Add($"{{{index}}}");
                        placeholders.Add(exprPart.Expression.ToString());
                        index++;
                    }
                }

                var extractedString = string.Join("", staticParts);
                interpolatedStrings.Add((node.ToString(), extractedString, placeholders));
            }

            return interpolatedStrings;
        }

        /// <summary>
        ///     Generates the resource file.
        /// </summary>
        /// <param name="extractedStrings">The extracted strings.</param>
        /// <param name="outputFilePath">The output file path.</param>
        /// <param name="appendToExisting">If true, appends to the existing file, otherwise overwrites it.</param>
        private static void GenerateResourceFile(IEnumerable<string> extractedStrings, string outputFilePath,
            bool appendToExisting = false)
        {
            var counter = 1;
            var resourceStrings = (from str in extractedStrings
                let resourceKey = $"Resource{counter++}"
                select $"public static readonly string {resourceKey} = \"{str}\";").ToList();

            // Ensure the file is a valid C# file
            if (appendToExisting && File.Exists(outputFilePath))
            {
                // Read the existing file to ensure we are appending within a valid class structure
                var existingCode = File.ReadAllText(outputFilePath);

                // Check if the file already contains a class with resources
                if (!existingCode.Contains("public static class Resource"))
                {
                    // If no class exists, create one and append
                    existingCode = existingCode.TrimEnd() + "\npublic static class Resource {\n" +
                                   string.Join("\n", resourceStrings) + "\n}\n";
                    File.WriteAllText(outputFilePath, existingCode);
                }
                else
                {
                    // Otherwise, append the resource strings to the existing class
                    var classStartIndex =
                        existingCode.IndexOf("public static class Resource", StringComparison.Ordinal);
                    var classEndIndex = existingCode.LastIndexOf("}", classStartIndex, StringComparison.Ordinal);

                    var beforeClass = existingCode.Substring(0, classEndIndex);
                    var afterClass = existingCode.Substring(classEndIndex);

                    // Append inside the class but outside of any existing methods
                    var updatedClass = beforeClass + "\n" + string.Join("\n", resourceStrings) + "\n" + afterClass;
                    File.WriteAllText(outputFilePath, updatedClass);
                }
            }
            else
            {
                // Create a new file (or overwrite if it exists)
                var newClassContent = "public static class Resource {\n" + string.Join("\n", resourceStrings) + "\n}";
                File.WriteAllText(outputFilePath, newClassContent);
            }
        }
    }
}
