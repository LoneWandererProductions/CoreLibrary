/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DoubleNewlineAnalyzer.cs
 * PURPOSE:     Simple Double Newline Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
///     Finds double line breaks.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class DoubleNewlineAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "DoubleNewline";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Simple Double Newline Analyzer.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // Skip ignored files
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        var lines = fileContent.Split('\n');
        for (var i = 1; i < lines.Length - 1; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(lines[i - 1]))
            {
                yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, i + 1,
                    "Multiple blank lines in a row.");
            }
        }
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        List<Diagnostic> diagnostics;

        try
        {
            diagnostics = AnalyzerExecutor.ExecutePath(
                this,
                args,
                "Usage: DoubleNewline <fileOrDirectoryPath>"
            );
        }
        catch (ArgumentException ae)
        {
            return CommandResult.Fail(ae.Message);
        }
        catch (FileNotFoundException fnfe)
        {
            return CommandResult.Fail(fnfe.Message);
        }

        if (diagnostics.Count == 0)
            return CommandResult.Ok($"No double newlines found in {args[0]}.");

        return FormatResult(diagnostics, Path.GetFileName(args[0]));
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }

    /// <summary>
    /// Formats diagnostic results into a CommandResult.
    /// </summary>
    /// <param name="diagnostics">The diagnostics.</param>
    /// <param name="target">The target.</param>
    /// <returns>Return result of our Input</returns>
    private static CommandResult FormatResult(List<Diagnostic> diagnostics, string target)
    {
        if (diagnostics.Count == 0)
            return CommandResult.Ok($"No issues found in {target}.");

        // Your Diagnostic already has a perfect ToString(), so we just join lines.
        var output = new StringBuilder()
            .AppendLine($"Found {diagnostics.Count} issue(s) in {target}:")
            .AppendJoin('\n', diagnostics.Select(d => d.ToString()))
            .ToString();

        return CommandResult.Ok(output);
    }
}
