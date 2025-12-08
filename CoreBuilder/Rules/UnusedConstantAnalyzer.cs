/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        UnusedConstantAnalyzer.cs
 * PURPOSE:     Analyzer to detect unused constants and static readonly fields across a project.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
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

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that detects unused constants and static readonly fields.
/// Works by scanning all files for declarations and then checking
/// whether those constants are referenced anywhere else in the project.
/// 
/// Limitations:
/// - Simple regex approach (not a full C# parser).
/// - May flag false positives if a constant is used via reflection, nameof(), etc.
/// - Project-wide scope is achieved by cross-file matching.
/// </summary>
public sealed class UnusedConstantAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "UnusedConstantAnalyzer";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer to detect unused constants and static readonly fields across a project.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// Runs a per-file analysis (not used in this analyzer).
    /// </summary>
    public IEnumerable<Diagnostic> Analyze(string filePath, string content)
    {
        // This analyzer does not work per file;
        // it only makes sense project-wide.
        return Enumerable.Empty<Diagnostic>();
    }

    /// <inheritdoc />
    public IEnumerable<Diagnostic> AnalyzeProject(Dictionary<string, string> allFiles)
    {
        var results = new List<Diagnostic>();

        // Regex to find constant or static readonly declarations
        // Matches e.g.: public const int Foo = 1; or private static readonly string Bar = "X";
        var declRegex = new Regex(@"\b(?:const|static\s+readonly)\s+\w[\w<>,\s]*\s+(?<name>\w+)\s*=",
            RegexOptions.Compiled);

        // Collect all declarations
        var declarations = new List<(string FilePath, int Line, string Name)>();

        foreach (var kvp in allFiles)
        {
            var filePath = kvp.Key;
            var content = kvp.Value;
            var lines = content.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var match = declRegex.Match(lines[i]);
                if (match.Success)
                {
                    declarations.Add((filePath, i + 1, match.Groups["name"].Value));
                }
            }
        }

        // Cross-check each declaration against all file contents
        foreach (var decl in declarations)
        {
            var usageCount = 0;

            foreach (var kvp in allFiles)
            {
                var lines = kvp.Value.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    // Skip the declaration line itself
                    if (kvp.Key == decl.FilePath && i == decl.Line - 1) continue;

                    usageCount += Regex.Matches(lines[i], $@"\b{Regex.Escape(decl.Name)}\b").Count;
                }
            }

            if (usageCount == 0)
            {
                results.Add(new Diagnostic(
                    decl.Name,
                    DiagnosticSeverity.Info,
                    decl.FilePath,
                    decl.Line,
                    $"Constant or static readonly field '{decl.Name}' is never used in the project."
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
            results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedConstantAnalyzer <fileOrDirectoryPath>");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(ex.Message);
        }

        var output = string.Join("\n", results.Select(d =>
                         $"{d.FilePath}({d.LineNumber}): {d.Message}"))
                     + $"\nTotal: {results.Count} unused private fields.";

        return CommandResult.Ok(output);
    }


    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"No extension '{extensionName}' exists for {Name}.");
    }
}
