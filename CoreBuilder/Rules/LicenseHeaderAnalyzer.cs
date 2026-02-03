/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Analyzer to detect missing license headers in C# files.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Detects missing license headers at the start of C# files.
    /// Implements ICommand for Weaver integration.
    /// </summary>
    public sealed class LicenseHeaderAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "LicenseHeader";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Simple analyzer that checks if a file starts with a license header.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        /// <summary>
        ///     Analyzes the file content and detects whether a license header is present.
        /// </summary>
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var trimmed = fileContent.TrimStart();
            var firstComment = ExtractFirstCommentBlock(trimmed);

            if (!HasLicenseHeader(firstComment))
            {
                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Info,
                    filePath,
                    1,
                    "Missing license header.");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Executes the analyzer on a directory path using centralized RunAnalyze logic.
        /// </summary>
        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: LicenseHeader <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var output = string.Join(Environment.NewLine,
                results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")
            );

            return CommandResult.Ok(output, results);
        }

        /// <summary>
        /// Extracts the first comment block (/* ... */) or contiguous line comments (// ...).
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Extrected block.</returns>
        private static string ExtractFirstCommentBlock(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            if (content.StartsWith("/*", StringComparison.Ordinal))
            {
                var endIdx = content.IndexOf("*/", StringComparison.Ordinal);
                return endIdx > 0 ? content[..(endIdx + 2)] : content;
            }

            if (content.StartsWith("//", StringComparison.Ordinal))
            {
                var lines = content.Split('\n')
                    .TakeWhile(line => line.TrimStart().StartsWith("//", StringComparison.Ordinal));
                return string.Join("\n", lines);
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines if a comment block contains license or copyright information.
        /// </summary>
        /// <param name="commentBlock">The comment block.</param>
        /// <returns>
        ///   <c>true</c> if [has license header] [the specified comment block]; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasLicenseHeader(string commentBlock)
        {
            if (string.IsNullOrEmpty(commentBlock))
                return false;

            var header = commentBlock.ToUpperInvariant();
            return header.Contains("COPYRIGHT") || header.Contains("LICENSE");
        }
    }
}