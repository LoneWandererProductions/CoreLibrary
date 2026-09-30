/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        EmptyCatchBlockAnalyzer.cs
 * PURPOSE:     Detects empty catch blocks that swallow exceptions silently.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using Core.Apps.Enums;
using Core.Apps.Helper;
using Core.Apps.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = Core.Apps.Enums.DiagnosticSeverity;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects empty catch blocks swallowing exceptions.
    /// </summary>
    public sealed class EmptyCatchBlockAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "EmptyCatchBlock";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects empty catch blocks that swallow exceptions silently.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var catchClause in root.DescendantNodes().OfType<CatchClauseSyntax>())
            {
                // Catch block is empty if it contains no statements
                if (catchClause.Block == null || catchClause.Block.Statements.Count == 0)
                {
                    var line = catchClause.CatchKeyword.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Warning,
                        filePath,
                        line,
                        "Empty catch block detected. Swallowing exceptions blindly hides critical runtime bugs.",
                        DiagnosticImpact.Readability
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: EmptyCatchBlock <fileOrDirectoryPath>");
                return CommandResult.Ok($"Empty catch blocks found: {results.Count}\n" +
                    string.Join("\n", results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")));
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}