/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Development
 * FILE:        HeaderExtractor.cs
 * PURPOSE:     Inserts or detects license headers in C# source files.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using CoreBuilder.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Development
{
    /// <inheritdoc />
    /// <summary>
    ///     Tool for detecting and inserting standardized license headers
    ///     into C# source files. Integrates with Weaver’s command infrastructure.
    /// </summary>
    public sealed class HeaderExtractor : ICommand
    {
        /// <summary>
        ///     Defines the header template with placeholders for file info.
        /// </summary>
        private const string HeaderTemplate = @"/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     {0}
 * FILE:        {1}
 * PURPOSE:     {2}
 * PROGRAMMER:  {3}
 */
";

        /// <inheritdoc />
        public string Name => "Header";

        /// <inheritdoc />
        public string Description => "Detects and inserts standardized license headers into C# files.";

        /// <inheritdoc />
        public string Namespace => "Development";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Processes the files, inserting headers where missing.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="includeSubdirectories">if set to <c>true</c> [include subdirectories].</param>
        /// <returns>Files with added headers</returns>
        public string ProcessFiles(string? directoryPath, bool includeSubdirectories)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return "Invalid or missing directory path.";

            var files = GetCandidateFiles(directoryPath, includeSubdirectories);
            var log = new StringBuilder();

            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);

                    if (ContainsHeader(content))
                    {
                        log.AppendLine($"Header already exists in {file}.");
                        continue;
                    }

                    var updatedContent = InsertHeader(
                        fileContent: content,
                        fileName: Path.GetFileName(file),
                        purpose: "Your file purpose here",
                        programmerName: "Peter Geinitz (Wayfarer)");

                    File.WriteAllText(file, updatedContent);
                    log.AppendLine($"Header inserted in {file}");
                }
                catch (Exception ex)
                {
                    log.AppendLine($"Failed to process {file}: {ex.Message}");
                }
            }

            return log.Length > 0 ? log.ToString() : "No files required header insertion.";
        }

        /// <summary>
        /// Detects the files needing headers.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="includeSubdirectories">if set to <c>true</c> [include subdirectories].</param>
        /// <returns>Files that need headers</returns>
        public string DetectFilesNeedingHeaders(string? directoryPath, bool includeSubdirectories)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return "Invalid directory path.";

            var files = GetCandidateFiles(directoryPath, includeSubdirectories);
            var needingHeaders = files
                .Where(file => !ContainsHeader(File.ReadAllText(file)))
                .ToList();

            return needingHeaders.Count > 0
                ? string.Join(Environment.NewLine, needingHeaders)
                : "All files already contain headers.";
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Missing argument: directory path.");

            var directoryPath = args[0];
            var includeSubdirs =
                args.Length <= 1 || (args.Length > 1 && bool.TryParse(args[1], out var result) && result);

            var resultMessage = ProcessFiles(directoryPath, includeSubdirs);
            return CommandResult.Ok(resultMessage);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Preview mode for "tryrun". Shows files that would be affected and
        ///     requests confirmation before executing.
        /// </summary>
        public CommandResult TryRun(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Missing argument: directory path.");

            var directoryPath = args[0];
            var includeSubdirs =
                args.Length <= 1 || (args.Length > 1 && bool.TryParse(args[1], out var result) && result);

            var previewList = DetectFilesNeedingHeaders(directoryPath, includeSubdirs);

            if (string.IsNullOrWhiteSpace(previewList) || previewList.StartsWith("All files"))
                return CommandResult.Ok("All files already contain headers. Nothing to insert.");

            FeedbackRequest? feedback = null;

            feedback = new FeedbackRequest(
                prompt:
                $"The following files are missing headers:\n\n{previewList}\n\nProceed with header insertion? (yes/no)",
                options: new[] { "yes", "no" },
                onRespond: input =>
                {
                    input = input.Trim().ToLowerInvariant();
                    return input switch
                    {
                        "yes" => Execute(args),
                        "no" => CommandResult.Fail("Operation cancelled by user."),
                        _ => new CommandResult
                        {
                            Message = "Please answer yes/no.",
                            RequiresConfirmation = true,
                            Feedback = feedback
                        }
                    };
                });

            return new CommandResult
            {
                Message =
                    $"Preview of header insertion complete. Files missing headers:\n\n{previewList}\n\nAwaiting user confirmation.",
                Feedback = feedback,
                RequiresConfirmation = true,
                Success = false
            };
        }

        /// <summary>
        /// Gets candidate C# files in a directory, excluding generated or ignored files.
        /// </summary>
        private static IEnumerable<string> GetCandidateFiles(string directoryPath, bool includeSubdirectories)
        {
            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directoryPath, "*.cs", searchOption)
                .Where(file => !CoreHelper.ShouldIgnoreFile(file));
        }

        /// <summary>
        /// Determines whether the specified content contains header.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content contains header; otherwise, <c>false</c>.
        /// </returns>
        private static bool ContainsHeader(string content)
        {
            return content.Split('\n')
                .Select(line => line.Trim().ToLowerInvariant())
                .Any(trimmed => trimmed.Contains("copyright", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Extracts the namespace from a C# source file.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Used namespace.</returns>
        private static string ExtractNamespace(string content)
        {
            foreach (var parts in from line in content.Split('\n')
                     select line.Trim()
                     into trimmed
                     where trimmed.StartsWith("namespace ", StringComparison.InvariantCultureIgnoreCase)
                     select trimmed.Split(new[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                return parts.Length > 1 ? parts[1] : "UnknownNamespace";
            }

            return "UnknownNamespace";
        }

        /// <summary>
        /// Inserts the header.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="purpose">The purpose.</param>
        /// <param name="programmerName">Name of the programmer.</param>
        /// <returns>The source file with inserted header</returns>
        private static string InsertHeader(string fileContent, string fileName, string purpose, string programmerName)
        {
            var namespaceName = ExtractNamespace(fileContent);
            var header = string.Format(HeaderTemplate, namespaceName, fileName, purpose, programmerName);
            return string.Concat(header, Environment.NewLine, fileContent);
        }
    }
}