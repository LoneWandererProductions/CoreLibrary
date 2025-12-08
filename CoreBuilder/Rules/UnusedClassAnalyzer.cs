/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        UnusedClassAnalyzer.cs
 * PURPOSE:     Analyzer that detects unused classes across a project.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects unused classes across a project.
    /// Works by scanning all files for class declarations and then checking
    /// whether those classes are referenced anywhere else in the project.
    /// 
    /// Limitations:
    /// - Simple regex approach (not a full C# parser).
    /// - May flag false positives if a class is used via reflection or dynamically.
    /// - Project-wide scope is achieved by cross-file matching.
    /// </summary>
    public sealed class UnusedClassAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedClassAnalyzer";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Analyzer to detect unused classes across a project.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string content)
        {
            // Not used per-file
            return Enumerable.Empty<Diagnostic>();
        }

        /// <summary>
        /// Analyze all files together.
        /// Override this only if your analyzer requires project-wide context.
        /// </summary>
        /// <param name="allFiles">List of files</param>
        /// <returns>Unused classes and results of the diagnosis.</returns>
        public IEnumerable<Diagnostic> AnalyzeProject(Dictionary<string, string> allFiles)
        {
            var results = new List<Diagnostic>();

            // Regex to find class declarations
            // Matches e.g.: public class Foo { ... }
            var classRegex = new Regex(@"\bclass\s+(?<name>\w+)\b", RegexOptions.Compiled);

            var declarations = new List<(string FilePath, int Line, string Name)>();

            foreach (var kvp in allFiles)
            {
                var filePath = kvp.Key;
                var content = kvp.Value;
                var lines = content.Split('\n');

                for (var i = 0; i < lines.Length; i++)
                {
                    var match = classRegex.Match(lines[i]);
                    if (match.Success)
                    {
                        declarations.Add((filePath, i + 1, match.Groups["name"].Value));
                    }
                }
            }

            // Check if each class is used anywhere
            foreach (var decl in declarations)
            {
                // Ignore self-match in declaration
                var usageCount = allFiles.Values.Sum(c =>
                    Regex.Matches(c, $@"\b{Regex.Escape(decl.Name)}\b").Count);

                if (usageCount <= 1) // only declaration itself
                {
                    results.Add(new Diagnostic(
                        decl.Name,
                        DiagnosticSeverity.Info,
                        decl.FilePath,
                        decl.Line,
                        $"Class '{decl.Name}' is never used in the project."
                    ));
                }
            }

            return results;
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedClassAnalyzer <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var report = string.Join(Environment.NewLine,
                results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}"));

            return CommandResult.Ok($"Unused Classes Report:\n{report}");
        }


        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"'{Name}' has no extensions.");
        }
    }
}
