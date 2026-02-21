/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CoreBuilder.Development
* FILE:        CoreBuilder/ResXtract.cs
* PURPOSE:     String Resource extractor.
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreBuilder.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;


namespace CoreBuilder.Development
{
    /// <inheritdoc />
    /// <summary>
    ///     Our Code resource refactor tool. In this case strings.
    /// </summary>
    /// <seealso cref="T:CoreBuilder.IResourceExtractor" />
    public sealed class ResXtract : ICommand
    {
        /// <summary>
        ///     The ignore list
        /// </summary>
        private readonly List<string> _ignoreList;

        /// <summary>
        ///     The ignore patterns
        /// </summary>
        private readonly List<string> _ignorePatterns;

        /// <inheritdoc />
        public string Name => "ResXtract";

        /// <inheritdoc />
        public string Description => "Extracts hardcoded strings into a resource file and optionally rewrites code.";

        /// <inheritdoc />
        public string Namespace => "Development";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

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
            return RunExtraction(projectPath, outputResourceFile, appendToExisting, replace).ChangedFiles;
        }

        /// <summary>
        /// Detects the affected files.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <returns>Files that would be affected.</returns>
        public string? DetectAffectedFiles(string? projectPath)
        {
            var files = CoreHelper.GetSourceFiles(projectPath);

            var affectedFiles =
                (from file in files
                    let code = File.ReadAllText(file)
                    let strings = ExtractStrings(code)
                    where strings.Any()
                    select Path.GetFullPath(file)).ToList();

            return affectedFiles.Count == 0
                ? null
                : string.Join(Environment.NewLine, affectedFiles);
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage:\n  ResXtract <projectPath>\n  ResXtract --detect <projectPath>");

            // detect mode
            if (args[0].Equals("--detect", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length < 2)
                    return CommandResult.Fail("Usage: ResXtract --detect <projectPath>");

                var projectPath = args[1];
                var result = DetectAffectedFiles(projectPath);

                if (string.IsNullOrWhiteSpace(result))
                    return CommandResult.Ok("No files would be changed.");

                return new CommandResult { Success = true, Message = "Files that would be affected:\n" + result };
            }

            // standard mode
            var changed = RunExtraction(args[0], null, appendToExisting: false, replace: true).ChangedFiles;
            var msg = changed.Count == 0
                ? "No files changed."
                : $"Updated {changed.Count} files:\n" + string.Join(Environment.NewLine, changed);

            return new CommandResult { Success = true, Message = msg };
        }

        /// <inheritdoc />
        public CommandResult TryRun(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage:\n  ResXtract <projectPath>\n  ResXtract --detect <projectPath>");

            var projectPath = args[0];
            var preview = DetectAffectedFiles(projectPath);

            if (string.IsNullOrWhiteSpace(preview))
                return CommandResult.Ok("No files would be changed.");

            // Step 1: create a feedback request for user confirmation
            FeedbackRequest? feedback = null;
            var cache = feedback;

            feedback = new FeedbackRequest(
                prompt: $"The following files would be updated:\n\n{preview}\n\nProceed? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => Execute(projectPath),
                        "no" => CommandResult.Fail("Operation cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no.",
                            RequiresConfirmation = true,
                            Feedback = cache // allow retry
                        }
                    };
                });

            // Step 2: return preview + feedback object
            return new CommandResult
            {
                Message = $"[Preview-WithTry] {preview}\nAwaiting user confirmation.",
                Feedback = feedback,
                RequiresConfirmation = true,
                Success = false
            };
        }

        /// <summary>
        /// Represents the result of an extraction run.
        /// </summary>
        private sealed record ExtractionResult(List<string> ChangedFiles, Dictionary<string, string> Map);

        /// <summary>
        /// Extracts the strings.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Strings to extract.</returns>
        internal static IEnumerable<string> ExtractStrings(string code)
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
                        case InterpolationSyntax:
                            formatString += $"{{{placeholderIndex++}}}";
                            break;
                    }
                }

                stringLiterals.Add(formatString);
            }

            return stringLiterals;
        }

        /// <summary>
        ///     Centralized logic for executing the full extraction pipeline.
        /// </summary>
        /// <param name="projectPath">The project root.</param>
        /// <param name="outputResourceFile">Optional target resource file path.</param>
        /// <param name="appendToExisting">Append to existing file.</param>
        /// <param name="replace">Rewrite code references.</param>
        /// <returns>ExtractionResult with changed files and messages.</returns>
        private static ExtractionResult RunExtraction(string? projectPath, string? outputResourceFile,
            bool appendToExisting, bool replace)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("Project path must not be null or empty.", nameof(projectPath));

            var files = CoreHelper.GetSourceFiles(projectPath);

            var allExtractedStrings = new HashSet<string>();
            var changedFiles = new List<string>();

            if (string.IsNullOrWhiteSpace(outputResourceFile))
            {
                outputResourceFile = Path.Combine(projectPath, "Resources.cs");
            }

            // 1️⃣ Extract all strings first
            foreach (var file in files)
            {
                var code = File.ReadAllText(file);
                var strings = ExtractStrings(code);

                foreach (var str in strings)
                    allExtractedStrings.Add(str);
            }

            // 2️⃣ Generate string-to-resource map
            var stringToResourceMap =
                GenerateResourceMap(allExtractedStrings, appendToExisting ? outputResourceFile : null);

            // 3️⃣ Optionally rewrite source files
            if (replace)
            {
                foreach (var file in files)
                {
                    var code = File.ReadAllText(file);
                    var rewrite = new StringLiteralRewrite(stringToResourceMap);
                    var rewrittenCode = rewrite.Rewrite(code);

                    if (code == rewrittenCode) continue;

                    File.WriteAllText(file, rewrittenCode);
                    changedFiles.Add(Path.GetFullPath(file));
                }
            }

            // 4️⃣ Generate the resource file
            GenerateResourceFile(stringToResourceMap, outputResourceFile, appendToExisting);
            changedFiles.Add(Path.GetFullPath(outputResourceFile));

            return new ExtractionResult(changedFiles, stringToResourceMap);
        }

        /// <summary>
        /// Generates the resource map.
        /// </summary>
        /// <param name="strings">The strings.</param>
        /// <param name="existingFile">The existing file.</param>
        /// <returns>Resource Map</returns>
        private static Dictionary<string, string> GenerateResourceMap(IEnumerable<string> strings,
            string? existingFile = null)
        {
            var map = new Dictionary<string, string>();
            var counter = 1;

            if (!string.IsNullOrEmpty(existingFile) && File.Exists(existingFile))
            {
                foreach (var line in File.ReadAllLines(existingFile))
                {
                    var trimmed = line.Trim();
                    if (!(trimmed.StartsWith("public static") || trimmed.StartsWith("internal static")))
                        continue;

                    var name = trimmed.Split('=')[0].Split(' ').Last();
                    var value = trimmed.Split('=')[1].Trim().Trim('"', ';');
                    map[value] = name;
                }

                counter = map.Count + 1;
            }

            foreach (var str in strings.Distinct())
            {
                if (!map.ContainsKey(str))
                    map[str] = $"Resource{counter++}";
            }

            return map;
        }

        /// <summary>
        /// Generates the resource file.
        /// </summary>
        /// <param name="resourceMap">The resource map.</param>
        /// <param name="outputFilePath">The output file path.</param>
        /// <param name="appendToExisting">if set to <c>true</c> [append to existing].</param>
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
    }
}
