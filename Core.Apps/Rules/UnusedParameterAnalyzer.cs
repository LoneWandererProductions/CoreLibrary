/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedParameterAnalyzer.cs
 * PURPOSE:     Unused parameter Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

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
    /// Analyzer that finds unused method parameters.
    /// </summary>
    public sealed class UnusedParameterAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedParameter";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Analyzer that finds unused method parameters.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            // 🔹 Ignore generated code and compiler artifacts
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            // Dynamically gather loaded assemblies as metadata references for full symbol resolution
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location));

            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(references)
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            foreach (var methodDecl in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                // Skip abstract/interface methods (no body to analyze)
                if (methodDecl.Body == null && methodDecl.ExpressionBody == null)
                    continue;

                var methodSymbol = model.GetDeclaredSymbol(methodDecl);
                if (methodSymbol == null)
                    continue;

                // Skip overrides, interface implementations, or virtual/abstract signatures
                if (methodSymbol.IsOverride || methodSymbol.IsAbstract || methodSymbol.ExplicitInterfaceImplementations.Length > 0)
                    continue;

                // Skip standard WPF/WinForms event handlers (e.g. sender, e)
                if (methodDecl.ParameterList.Parameters.Count == 2 &&
                    methodDecl.ParameterList.Parameters[1].Type?.ToString().EndsWith("EventArgs") == true)
                    continue;

                foreach (var parameter in methodDecl.ParameterList.Parameters)
                {
                    // Skip discard parameters
                    if (parameter.Identifier.Text.StartsWith("_"))
                        continue;

                    var symbol = model.GetDeclaredSymbol(parameter);
                    if (symbol is not { } paramSymbol)
                        continue;

                    var referencesList = methodDecl.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id =>
                            SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, paramSymbol));

                    if (referencesList.Any())
                        continue;

                    var line = parameter.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(Name, DiagnosticSeverity.Warning, filePath, line,
                        $"Unused parameter '{parameter.Identifier.Text}' in method '{methodDecl.Identifier.Text}'.");
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedParameter <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var output = string.Join("\n", results.Select(d =>
                             $"{d.FilePath}({d.LineNumber}): {d.Message}")) +
                         $"\nTotal: {results.Count} unused parameters.";

            return CommandResult.Ok(output);
        }
    }
}