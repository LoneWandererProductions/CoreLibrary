/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        RethrowExceptionAnalyzer.cs
 * PURPOSE:     Detects 'throw ex;' statements that destroy exception stack traces.
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
    /// Analyzer that detects 'throw ex;' statements inside catch blocks.
    /// </summary>
    public sealed class RethrowExceptionAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "RethrowException";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects 'throw ex;' statements that destroy the original exception stack trace.";

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

            foreach (var throwStmt in root.DescendantNodes().OfType<ThrowStatementSyntax>())
            {
                // Check if throwing an explicit variable identifier (e.g. throw ex;)
                if (throwStmt.Expression is IdentifierNameSyntax thrownIdentifier)
                {
                    var catchClause = throwStmt.Ancestors().OfType<CatchClauseSyntax>().FirstOrDefault();
                    if (catchClause?.Declaration != null &&
                        catchClause.Declaration.Identifier.Text == thrownIdentifier.Identifier.Text)
                    {
                        var line = throwStmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Warning,
                            filePath,
                            line,
                            $"Rethrowing '{thrownIdentifier.Identifier.Text}' with 'throw {thrownIdentifier.Identifier.Text};' resets the stack trace. Use 'throw;' instead.",
                            DiagnosticImpact.Readability
                        );
                    }
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: RethrowException <fileOrDirectoryPath>");
                return CommandResult.Ok($"Stack-trace destroying rethrows found: {results.Count}\n" +
                    string.Join("\n", results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")));
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}