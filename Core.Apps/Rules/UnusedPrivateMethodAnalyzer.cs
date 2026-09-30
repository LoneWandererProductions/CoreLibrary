/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedPrivateMethodAnalyzer.cs
 * PURPOSE:     Unused private methods Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
    /// Detects private methods that are never called within the containing file.
    /// </summary>
    public sealed class UnusedPrivateMethodAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedPrivateMethod";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects unreferenced private methods in a file.";

        /// <inheritdoc cref="ICodeAnalyzer" />
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
                var symbol = model.GetDeclaredSymbol(methodDecl);
                if (symbol == null)
                    continue;

                // Ensure it is private
                if (symbol.DeclaredAccessibility != Microsoft.CodeAnalysis.Accessibility.Private)
                    continue;

                // Exclude entry points, constructors, overrides, or interface implementations
                if (symbol.Name == "Main" || symbol.IsOverride || symbol.ExplicitInterfaceImplementations.Length > 0)
                    continue;

                // Check for usages (invocations, method group conversions, nameof, etc.)
                var isReferenced = root.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(id => id.Parent != methodDecl && id.Identifier.Text == symbol.Name)
                    .Any(id => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, symbol));

                if (!isReferenced)
                {
                    var line = methodDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Private method '{symbol.Name}' is never referenced.",
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
                var results =
                    AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedPrivateMethod <fileOrDirectoryPath>");
                return CommandResult.Ok($"Unused private methods found: {results.Count}\n" +
                                        string.Join("\n",
                                            results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")));
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
