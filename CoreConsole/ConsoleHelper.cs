/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreConsole
 * FILE:        ConsoleHelper.cs
 * PURPOSE:     Sorting out the actual programs to keep the console clean.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreBuilder;
using CoreBuilder.Interface;
using Interpreter;

namespace CoreConsole;

/// <summary>
///     Provides helper methods for console-based command execution.
/// </summary>
internal static class ConsoleHelper
{
    /// <summary>
    ///     List of code analyzers to apply.
    /// </summary>
    private static readonly List<ICodeAnalyzer> Analyzers = new();

    /// <summary>
    ///     Static constructor to initialize analyzers.
    /// </summary>
    static ConsoleHelper()
    {
        Analyzers.Add(new DoubleNewlineAnalyzer());
        Analyzers.Add(new LicenseHeaderAnalyzer());

        // New Roslyn-based analyzers
        Analyzers.Add(new UnusedLocalVariableAnalyzer());
        Analyzers.Add(new UnusedParameterAnalyzer());
        Analyzers.Add(new UnusedPrivateFieldAnalyzer());
        Analyzers.Add(new HotPathAnalyzer());
        Analyzers.Add(new AllocationAnalyzer());
        Analyzers.Add(new DisposableAnalyzer());
        Analyzers.Add(new EventHandlerAnalyzer());
        Analyzers.Add(new UnusedConstantAnalyzer());
        Analyzers.Add(new UnusedClassAnalyzer());
    }

    /// <summary>
    ///     Applies license headers to files in a given directory.
    /// </summary>
    /// <param name="package">The command package with parameters.</param>
    /// <returns>Status message indicating result.</returns>
    internal static string HandleHeader(OutCommand package)
    {
        var directoryPath = CleanPath(package.Parameter[0]);
        if (!Directory.Exists(directoryPath))
        {
            return string.Format(ConResources.ErrorDirectory, directoryPath);
        }

        IHeaderExtractor headerExtractor = new HeaderExtractor();
        return headerExtractor.ProcessFiles(directoryPath, true);
    }

    /// <summary>
    ///     Cleans a path string by removing enclosing double quotes and trimming whitespace.
    /// </summary>
    /// <param name="path">The path string.</param>
    /// <returns>Cleaned path string.</returns>
    private static string? CleanPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        path = path.Trim();
        if (path.StartsWith(ConResources.Quotes, StringComparison.Ordinal) &&
            path.EndsWith(ConResources.Quotes, StringComparison.Ordinal) &&
            path.Length > 1)
        {
            path = path.Substring(1, path.Length - 2);
        }

        return path;
    }

    /// <summary>
    ///     Validates and extracts a valid project path from a command.
    /// </summary>
    /// <param name="command">The command containing path parameters.</param>
    /// <param name="projectPath">Extracted project path.</param>
    /// <param name="error">Error message if invalid.</param>
    /// <returns>True if valid; otherwise false.</returns>
    private static bool TryGetValidProjectPath(OutCommand command, out string? projectPath, out string error)
    {
        error = string.Empty;
        projectPath = command.Parameter.Count > 0 ? CleanPath(command.Parameter[0]) : null;

        if (string.IsNullOrWhiteSpace(projectPath))
        {
            error = ConResources.ErrorProjectPathMissing;
            return false;
        }

        if (!Directory.Exists(projectPath))
        {
            error = string.Format(ConResources.ErrorProjectPath, projectPath);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Validates the output file path.
    /// </summary>
    /// <param name="outputResourceFile">The output file path.</param>
    /// <param name="error">Error message if invalid.</param>
    /// <returns>True if valid; otherwise false.</returns>
    private static bool TryValidateOutputFilePath(string outputResourceFile, out string error)
    {
        error = string.Empty;

        try
        {
            var outputDir = Path.GetDirectoryName(outputResourceFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                error = string.Format(ConResources.ErrorDirectoryOutput, outputDir);
                return false;
            }

            if (File.Exists(outputResourceFile))
            {
                // Optional: log a warning or inform the user
            }
        }
        catch (Exception ex)
        {
            error = string.Format(ConResources.ErrorAccessFile, ex.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Creates the default resource extractor.
    /// </summary>
    /// <returns>New resource extractor instance.</returns>
    private static IResourceExtractor CreateDefaultResourceExtractor()
    {
        return new ResXtract(new List<string>(), new List<string>());
    }

    /// <summary>
    ///     Extracts resources and applies changes to the project.
    /// </summary>
    /// <param name="package">The command containing project and output path.</param>
    /// <returns>Result message.</returns>
    internal static string HandleResxtract(OutCommand package)
    {
        if (!TryGetValidProjectPath(package, out var projectPath, out var error))
        {
            return error;
        }

        var outputResourceFile = package.Parameter.Count >= 2 ? CleanPath(package.Parameter[1]) : null;

        if (!string.IsNullOrWhiteSpace(outputResourceFile) &&
            !TryValidateOutputFilePath(outputResourceFile, out var outputError))
        {
            return outputError;
        }

        var extractor = CreateDefaultResourceExtractor();
        var changedFiles = extractor.ProcessProject(projectPath, outputResourceFile, replace: true);

        if (changedFiles.Count == 0)
        {
            return ConResources.ResxtractFinished;
        }

        var actualOutputFile = changedFiles.Last();
        var changedFilesList = string.Join(Environment.NewLine + ConResources.MessageSeparator,
            changedFiles.Take(changedFiles.Count - 1));

        return string.Format(ConResources.ResourceResxtractOutput, actualOutputFile, Environment.NewLine) +
               string.Format(ConResources.MessageChangedFiles, Environment.NewLine, changedFilesList);
    }

    /// <summary>
    ///     Runs all analyzers on all .cs files within the provided directory.
    /// </summary>
    /// <param name="package">The command containing the directory path.</param>
    /// <returns>Diagnostics result from analyzers.</returns>
    internal static (IReadOnlyList<Diagnostic> Diagnostics, string Output) RunAnalyzers(OutCommand package)
    {
        var path = CleanPath(package.Parameter[0]);
        var diagnostics = new List<Diagnostic>();

        if (!Directory.Exists(path))
        {
            diagnostics.Add(new Diagnostic(ConResources.MessageError, DiagnosticSeverity.Error, path, -1,
                string.Format(ConResources.ErrorDirectory, path)));
        }
        else
        {
            var files = Directory.GetFiles(path, ConResources.ResourceCsExtension, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                diagnostics.AddRange(Analyzers.SelectMany(analyzer => analyzer.Analyze(file, content)));
            }
        }

        var output = string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));
        return (diagnostics, output);
    }


    /// <summary>
    ///     Simulates applying license headers to preview affected files.
    /// </summary>
    /// <param name="outCommand">The command containing the directory path.</param>
    /// <returns>Preview of changes.</returns>
    internal static string HandleHeaderTryrun(OutCommand outCommand)
    {
        var directoryPath = CleanPath(outCommand.Parameter[0]);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return ConResources.InformationDirectoryMissing;
        }

        if (!Directory.Exists(directoryPath))
        {
            return string.Format(ConResources.ErrorDirectory, directoryPath);
        }

        IHeaderExtractor headerExtractor = new HeaderExtractor();
        var changedFiles = headerExtractor.DetectFilesNeedingHeaders(directoryPath, true);

        return string.IsNullOrEmpty(changedFiles)
            ? ConResources.HeaderTryrunNoChanges
            : string.Concat(ConResources.HeaderTryrunWouldAffect, changedFiles);
    }

    /// <summary>
    ///     Simulates resource extraction and lists affected files.
    /// </summary>
    /// <param name="outCommand">The command containing the project path.</param>
    /// <returns>Preview of changes.</returns>
    internal static string HandleResxtractTryrun(OutCommand outCommand)
    {
        if (!TryGetValidProjectPath(outCommand, out var projectPath, out var error))
        {
            return error;
        }

        var extractor = CreateDefaultResourceExtractor();
        var simulatedChanges = extractor.DetectAffectedFiles(projectPath);

        return string.IsNullOrEmpty(simulatedChanges)
            ? ConResources.HeaderTryrunNoChanges
            : string.Concat(ConResources.ResxtractTryrunWouldAffect, simulatedChanges);
    }

    /// <summary>
    ///     Lists all files in the given directory with size and percentage of total.
    /// </summary>
    /// <param name="package">The command containing the directory path.</param>
    /// <returns>Formatted result string with file sizes and their percentages.</returns>
    internal static string HandleDirAnalyzer(OutCommand package)
    {
        var directoryPath = CleanPath(package.Parameter[0]);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return ConResources.InformationDirectoryMissing;
        }

        if (!Directory.Exists(directoryPath))
        {
            return string.Format(ConResources.ErrorDirectory, directoryPath);
        }

        IDirectorySizeAnalyzer analyzer = new DirectorySizeAnalyzer();
        return analyzer.DisplayDirectorySizeOverview(directoryPath, true); // true = include subdirs
    }
}
