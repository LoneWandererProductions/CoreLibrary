/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        UnusedClassAnalyzer.cs
 * PURPOSE:     Analyzer that detects unused classes across a project.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CoreBuilder.Interface;

namespace CoreBuilder
{
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
    public sealed class UnusedClassAnalyzer : ICodeAnalyzer
    {
        /// <inheritdoc />
        public string Name => "UnusedClassAnalyzer";

        /// <inheritdoc />
        public string Description => "Analyzer to detect unused classes across a project.";

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
        /// <param name="allFiles"></param>
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
                var isUsed = allFiles.Values.Any(c =>
                    Regex.IsMatch(c, $@"\b{Regex.Escape(decl.Name)}\b"));

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
    }
}
