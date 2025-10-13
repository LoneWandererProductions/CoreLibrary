/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/EmptyCatchBlockAnalyzer.cs
 * PURPOSE:     Detects empty catch blocks.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules;

/// <summary>
/// Detects empty catch blocks (that silently swallow exceptions).
/// </summary>
public sealed class EmptyCatchBlockAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => nameof(EmptyCatchBlockAnalyzer);

    /// <inheritdoc />
    public string Description => "Detects catch blocks that are empty (swallowing exceptions).";

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(fileContent))
        {
            yield break;
        }

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        foreach (var catchClause in root.DescendantNodes().OfType<CatchClauseSyntax>())
        {
            // Block exists but has no statements
            if (catchClause.Block?.Statements.Any() == false)
            {
                var lineNumber = catchClause.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Warning,
                    filePath,
                    lineNumber,
                    "Empty catch block detected. Consider logging or handling the exception properly.",
                    DiagnosticImpact.Logic
                );
            }
        }
    }
}
