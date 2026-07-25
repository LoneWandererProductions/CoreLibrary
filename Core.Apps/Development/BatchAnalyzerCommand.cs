/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Development
 * FILE:        BatchAnalyzerCommand.cs
 * PURPOSE:     Runs all code analyzers on a target folder and exports diagnostics to a CSV report.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Apps.Helper;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Core.Apps.Development
{
    /// <inheritdoc cref="ICommand" />
    /// <summary>
    /// Executes all available code analyzers across all C# files in a provided directory 
    /// and exports the aggregated results into a CSV file in the local working directory.
    /// </summary>
    public sealed class BatchAnalyzerCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "batchanalyze";

        /// <inheritdoc />
        public string Namespace => "Development";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public string Description => "Runs all code analyzers on a directory and exports diagnostics to a CSV file.";

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: batchanalyze <root_folder>");

            var rootPath = args[0];
            if (!Directory.Exists(rootPath))
                return CommandResult.Fail("Folder not found.");

            var analyzers = CommandFactory.GetAllAnalyzers();
            if (analyzers == null || analyzers.Count == 0)
                return CommandResult.Fail("No analyzers found via CommandFactory.");

            var allDiagnostics = new List<Diagnostic>();
            var csFiles = Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories).ToList();

            foreach (var filePath in csFiles)
            {
                // Skip generated or ignored files if CoreHelper supports it
                if (CoreHelper.ShouldIgnoreFile(filePath))
                    continue;

                string fileContent;
                try
                {
                    fileContent = File.ReadAllText(filePath);
                }
                catch
                {
                    continue; // Skip files that cannot be read safely
                }

                foreach (var analyzer in analyzers)
                {
                    try
                    {
                        var diagnostics = analyzer.Analyze(filePath, fileContent);
                        if (diagnostics != null)
                        {
                            allDiagnostics.AddRange(diagnostics);
                        }
                    }
                    catch
                    {
                        // Prevent an exception in a single analyzer from crashing the batch run
                    }
                }
            }

            // Build CSV file path in the local directory with a timestamp to prevent overwriting
            var csvFileName = $"AnalysisReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), csvFileName);

            var csvBuilder = new StringBuilder();
            // CSV Header with FileName included
            csvBuilder.AppendLine("Analyzer,Severity,FileName,File,LineNumber,Message,Impact");

            foreach (var d in allDiagnostics)
            {
                // Properly escape strings containing quotes or commas for safe CSV formatting
                var escapedMessage = $"\"{d.Message.Replace("\"", "\"\"")}\"";
                var escapedFile = $"\"{d.FilePath}\"";
                var escapedFileName = $"\"{d.FileName}\"";

                csvBuilder.AppendLine($"{d.Name},{d.Severity},{escapedFileName},{escapedFile},{d.LineNumber},{escapedMessage},{d.Impact}");
            }

            try
            {
                File.WriteAllText(csvPath, csvBuilder.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Failed to write CSV report: {ex.Message}");
            }

            return CommandResult.Ok(
                $"Successfully analyzed {csFiles.Count} files using {analyzers.Count} analyzers. Report saved to: {csvPath}",
                EnumTypes.Wstring
            );
        }
    }
}