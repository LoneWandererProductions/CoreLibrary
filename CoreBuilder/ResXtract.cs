using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder
{
    /// <summary>
    /// Small tool to create string Resource files
    /// </summary>
    /// <seealso cref="CoreBuilder.IResourceExtractor" />
    public sealed class ResXtract : IResourceExtractor
    {
        private readonly List<string> _ignoreList;
        private readonly List<Regex> _ignorePatterns;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResXtract"/> class.
        /// </summary>
        /// <param name="ignoreList">The ignore list.</param>
        /// <param name="ignorePatterns">The ignore patterns.</param>
        public ResXtract(List<string> ignoreList = null, List<Regex> ignorePatterns = null)
        {
            _ignoreList = ignoreList ?? new List<string>();
            _ignorePatterns = ignorePatterns ?? new List<Regex>();
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the given project directory, extracting string literals
        /// and replacing them with resource references.
        /// </summary>
        /// <param name="projectPath">The root directory of the C# project.</param>
        /// <param name="outputResourceFile">Path to generate the resource file.</param>
        public void ProcessProject(string projectPath, string outputResourceFile)
        {
            var files = Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".xaml", StringComparison.Ordinal)); // Exclude XAML files
            var extractedStrings = new List<string>();

            foreach (var code in from file in files where !ShouldIgnoreFile(file) select File.ReadAllText(file))
            {
                extractedStrings.AddRange(ExtractStrings(code));
            }

            GenerateResourceFile(extractedStrings.Distinct().ToList(), outputResourceFile);
        }

        /// <summary>
        /// Should ignore file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public bool ShouldIgnoreFile(string filePath)
        {
            return _ignoreList.Contains(filePath) || _ignorePatterns.Any(pattern => pattern.IsMatch(filePath));
        }

        /// <summary>
        /// Extracts the strings.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public IEnumerable<string> ExtractStrings(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            var stringLiterals = new List<string>();
            var interpolatedStrings = ExtractInterpolatedStrings(code);

            // Add regular string literals
            stringLiterals.AddRange(root.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
                .Select(l => l.Token.ValueText));

            // Add interpolated string literals (only the extracted format string)
            foreach (var (_, extracted, _) in interpolatedStrings) // Discarding the `original` and `placeholders`
            {
                stringLiterals.Add(extracted);
            }

            return stringLiterals;
        }

        /// <summary>
        /// Extracts the interpolated strings.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        private IEnumerable<(string original, string extracted, List<string> placeholders)> ExtractInterpolatedStrings(
            string code)
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
        /// Generates the resource file.
        /// </summary>
        /// <param name="extractedStrings">The extracted strings.</param>
        /// <param name="outputFilePath">The output file path.</param>
        public static void GenerateResourceFile(IEnumerable<string> extractedStrings, string outputFilePath)
        {
            var counter = 1;

            var resourceStrings = (from str in extractedStrings
                let resourceKey = $"Resource{counter++}"
                select $"public static readonly string {resourceKey} = \"{str}\";").ToList();

            // Write to the output resource file
            File.WriteAllLines(outputFilePath, resourceStrings);
        }
    }
}
