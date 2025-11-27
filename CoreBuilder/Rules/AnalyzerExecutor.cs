/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        AnalyzerExecutor.cs
 * PURPOSE:     Helper to execute analyzers on files or directories.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreBuilder.Rules
{
    /// <summary>
    /// Analyzer Executor Helper.
    /// </summary>
    internal static class AnalyzerExecutor
    {
        /// <summary>
        /// Executes the path.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="usageMessage">The usage message.</param>
        /// <returns>Folders or for a File Path it will return a Diagnostic result, if any errors it will return a message.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.IO.FileNotFoundException">Path not found: {path}</exception>
        internal static List<Diagnostic> ExecutePath(ICodeAnalyzer? analyzer, string[] args, string usageMessage)
        {
            if (args.Length == 0)
                throw new ArgumentException(usageMessage);

            var path = args[0];
            if (!File.Exists(path) && !Directory.Exists(path))
                throw new FileNotFoundException($"Path not found: {path}", path);

            return Directory.Exists(path)
                ? RunAnalyze.RunAnalyzer(path, analyzer)?.ToList() ?? new List<Diagnostic>()
                : RunAnalyze.RunAnalyzerForFile(path, analyzer)?.ToList() ?? new List<Diagnostic>();
        }
    }
}