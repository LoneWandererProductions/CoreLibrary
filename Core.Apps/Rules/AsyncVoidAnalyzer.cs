/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        AsyncVoidAnalyzer.cs
 * PURPOSE:     Detects dangerous async void methods.
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
    /// Analyzer that detects dangerous async void methods.
    /// </summary>
    public sealed class AsyncVoidAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "AsyncVoid";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects dangerous async void methods that can crash the application process.";

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

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                // Check if method has the 'async' modifier
                if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword))
                    continue;

                // Check if return type is void
                if (method.ReturnType is PredefinedTypeSyntax voidType &&
                    voidType.Keyword.IsKind(SyntaxKind.VoidKeyword))
                {
                    // Ignore standard UI event handlers (e.g., void Button_Click(object sender, EventArgs e))
                    if (method.ParameterList.Parameters.Count == 2 &&
                        method.ParameterList.Parameters[1].Type?.ToString().EndsWith("EventArgs") == true)
                        continue;

                    var line = method.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Error,
                        filePath,
                        line,
                        $"Async method '{method.Identifier.Text}' returns void instead of Task. Unhandled exceptions will crash the process.",
                        DiagnosticImpact.Other
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: AsyncVoid <fileOrDirectoryPath>");
                return CommandResult.Ok($"Async void methods found: {results.Count}\n" +
                    string.Join("\n", results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")));
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}