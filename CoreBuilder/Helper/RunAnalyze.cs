/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        CoreHelper.cs
 * PURPOSE:     Runner that executes analyzers over directories or single files, is used in the command Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreBuilder.Helper
{
    /// <summary>
    /// Provides helper methods to execute analyzers over directories or single files.
    /// </summary>
    internal static class RunAnalyze
    {
        /// <summary>
        /// Runs the given analyzer against all C# source files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory path to scan recursively.</param>
        /// <param name="analyzer">The analyzer instance implementing <see cref="ICodeAnalyzer"/>.</param>
        /// <returns>A collection of all diagnostics produced by the analyzer.</returns>
        internal static IEnumerable<Diagnostic> RunAnalyzer(string directory, ICodeAnalyzer? analyzer)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(directory) || analyzer == null)
                return Enumerable.Empty<Diagnostic>();

            if (!Directory.Exists(directory))
                return Enumerable.Empty<Diagnostic>();

            var diagnostics = new List<Diagnostic>();

            // Search recursively for .cs files
            foreach (var file in CoreHelper.SafeEnumerateFiles(directory, CoreResources.ResourceCsExtension))
            {
                if (CoreHelper.ShouldIgnoreFile(file))
                    continue;

                var content = File.ReadAllText(file);
                diagnostics.AddRange(analyzer.Analyze(file, content));
            }

            return diagnostics;
        }

        /// <summary>
        /// Runs the analyzer on a single file, if it exists.
        /// </summary>
        /// <param name="filePath">The full path of the file to analyze.</param>
        /// <param name="analyzer">The analyzer instance implementing <see cref="ICodeAnalyzer"/>.</param>
        /// <returns>A collection of diagnostics for that file.</returns>
        internal static IEnumerable<Diagnostic> RunAnalyzerForFile(string filePath, ICodeAnalyzer? analyzer)
        {
            if (string.IsNullOrWhiteSpace(filePath) || analyzer == null)
                return Enumerable.Empty<Diagnostic>();

            if (!File.Exists(filePath))
                return Enumerable.Empty<Diagnostic>();

            if (CoreHelper.ShouldIgnoreFile(filePath))
                return Enumerable.Empty<Diagnostic>();

            var content = File.ReadAllText(filePath);
            return analyzer.Analyze(filePath, content);
        }
    }
}
