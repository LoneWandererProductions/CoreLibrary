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
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder;

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
    public ResXtract(List<string>? ignoreList = null, List<string>? ignorePatterns = null)
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
    /// <param name="replace">if set to <c>true</c> [replace].</param>
    /// <returns>List of changed Files with directory.</returns>
    public List<string> ProcessProject(string? projectPath, string? outputResourceFile = null,
        bool appendToExisting = false, bool replace = false)
    {
        var files = GetFiles(projectPath);

        var allExtractedStrings = new HashSet<string>();
        var changedFiles = new List<string>();

        if (string.IsNullOrWhiteSpace(outputResourceFile))
        {
            var defaultName = Path.Combine(projectPath, "Resources.cs");
            outputResourceFile = Path.Combine(projectPath, defaultName);
        }

        // 1️⃣ Extract all strings first
        foreach (var file in files)
        {
            if (ShouldIgnoreFile(file))
            {
                continue;
            }

            var code = File.ReadAllText(file);
            var strings = ExtractStrings(code);

            foreach (var str in strings)
            {
                allExtractedStrings.Add(str);
            }
        }

        // 2️⃣ Generate string-to-resource map
        var stringToResourceMap =
            GenerateResourceMap(allExtractedStrings, appendToExisting ? outputResourceFile : null);

        if (replace)
        {
            // 3️⃣ Rewrite source files
            foreach (var file in files)
            {
                if (ShouldIgnoreFile(file))
                {
                    continue;
                }

                var code = File.ReadAllText(file);
                var rewrite = new StringLiteralRewrite(stringToResourceMap);
                var rewrittenCode = rewrite.Rewrite(code);

                if (code == rewrittenCode)
                {
                    continue;
                }

                File.WriteAllText(file, rewrittenCode);
                changedFiles.Add(Path.GetFullPath(file));
            }
        }

        // 4️⃣ Generate the resource file
        GenerateResourceFile(stringToResourceMap, outputResourceFile, appendToExisting);
        changedFiles.Add(Path.GetFullPath(outputResourceFile));

        return changedFiles;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Simulates a dry-run of the resource extraction process, showing which files would be affected.
    /// </summary>
    /// <param name="projectPath">The root directory of the C# project.</param>
    /// <returns>A formatted string of files that would be changed.</returns>
    public string? DetectAffectedFiles(string? projectPath)
    {
        var files = GetFiles(projectPath);

        var affectedFiles =
            (from file in files
             where !ShouldIgnoreFile(file)
             let code = File.ReadAllText(file)
             let strings = ExtractStrings(code)
             where strings.Any()
             select Path.GetFullPath(file)).ToList();

        return affectedFiles.Count == 0
            ? null
            : string.Join(Environment.NewLine, affectedFiles);
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
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();

        var stringLiterals = new List<string>();

        // Extract regular string literals
        stringLiterals.AddRange(root.DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression))
            .Select(l => l.Token.ValueText));

        // Extract interpolated strings and convert them to format strings
        var interpolatedStrings = root.DescendantNodes()
            .OfType<InterpolatedStringExpressionSyntax>();

        foreach (var interpolated in interpolatedStrings)
        {
            var placeholderIndex = 0;
            var formatString = "";

            foreach (var content in interpolated.Contents)
            {
                switch (content)
                {
                    case InterpolatedStringTextSyntax textPart:
                        formatString += textPart.TextToken.ValueText;
                        break;
                    case InterpolationSyntax interpolationPart:
                        formatString += "{" + placeholderIndex + "}";
                        placeholderIndex++;
                        break;
                }
            }

            stringLiterals.Add(formatString);
        }

        return stringLiterals;
    }

    /// <summary>
    ///     Generates the resource map.
    ///     Generates a mapping of string to Resource key
    /// </summary>
    /// <param name="strings">The strings.</param>
    /// <param name="existingFile">The existing file.</param>
    /// <returns>Extracted strings.</returns>
    private static Dictionary<string, string> GenerateResourceMap(IEnumerable<string> strings,
        string? existingFile = null)
    {
        var map = new Dictionary<string, string>();
        var counter = 1;

        if (!string.IsNullOrEmpty(existingFile) && File.Exists(existingFile))
        {
            // Parse existing keys if appending
            foreach (var line in File.ReadAllLines(existingFile))
            {
                var trimmed = line.Trim();

                if (!(trimmed.StartsWith("public static readonly string Resource") ||
                      trimmed.StartsWith("internal static readonly string Resource") ||
                      trimmed.StartsWith("internal const string Resource")))
                {
                    continue;
                }

                var name = trimmed.Split('=')[0].Split(' ').Last();
                var value = trimmed.Split('=')[1].Trim().Trim('"', ';');
                map[value] = name;
            }

            counter = map.Count + 1;
        }

        foreach (var str in strings.Distinct())
        {
            if (!map.ContainsKey(str))
            {
                map[str] = $"Resource{counter++}";
            }
        }

        return map;
    }

    /// <summary>
    ///     Generates the resource file.
    /// </summary>
    /// <param name="resourceMap">The extracted strings.</param>
    /// <param name="outputFilePath">The output file path.</param>
    /// <param name="appendToExisting">If true, appends to the existing file, otherwise overwrites it.</param>
    private static void GenerateResourceFile(Dictionary<string, string> resourceMap, string? outputFilePath,
        bool appendToExisting)
    {
        var resourceEntries = resourceMap.OrderBy(kvp => kvp.Value)
            .Select(kvp => $"internal const string {kvp.Value} = \"{kvp.Key.Replace("\"", "\\\"")}\";")
            .ToList();

        if (appendToExisting && File.Exists(outputFilePath))
        {
            var existingCode = File.ReadAllText(outputFilePath);

            if (!existingCode.Contains("public static class Resource"))
            {
                existingCode = existingCode.TrimEnd() + "\npublic static class Resource {\n" +
                               string.Join("\n", resourceEntries) + "\n}\n";
            }
            else
            {
                var classEndIndex = existingCode.LastIndexOf('}');
                var updated = existingCode.Substring(0, classEndIndex) +
                              "\n" + string.Join("\n", resourceEntries) + "\n" +
                              existingCode.Substring(classEndIndex);
                existingCode = updated;
            }

            File.WriteAllText(outputFilePath, existingCode);
        }
        else
        {
            var classDef = "public static class Resource {\n" +
                           string.Join("\n", resourceEntries) + "\n}";
            File.WriteAllText(outputFilePath, classDef);
        }
    }

    /// <summary>
    ///     Gets the files.
    /// </summary>
    /// <param name="projectPath">The project path.</param>
    /// <returns>List of allowed files</returns>
    private static IEnumerable<string> GetFiles(string? projectPath)
    {
        return Directory.EnumerateFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("resource", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("const", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\bin\", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\.vs\", StringComparison.OrdinalIgnoreCase));
    }
}
