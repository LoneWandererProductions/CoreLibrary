/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        UnusedPrivateFieldAnalyzer.cs
 * PURPOSE:     Unused private field Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = CoreBuilder.Enums.DiagnosticSeverity;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that finds unused private fields.
    /// </summary>
    public sealed class UnusedPrivateFieldAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedPrivateField";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Analyzer that finds unused private fields.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            // 🔹 Ignore generated code and compiler artifacts
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            foreach (var fieldDecl in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                // Only look at private fields
                if (!fieldDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                    continue;

                foreach (var variable in fieldDecl.Declaration.Variables)
                {
                    var symbol = model.GetDeclaredSymbol(variable);
                    if (symbol is not IFieldSymbol fieldSymbol)
                        continue;

                    var references = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id => SymbolEqualityComparer.Default.Equals(
                            model.GetSymbolInfo(id).Symbol, fieldSymbol));

                    if (references.Any())
                        continue;

                    var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                        $"Unused private field '{variable.Identifier.Text}'.");
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedPrivateField <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var output = string.Join("\n", results.Select(d =>
                             $"{d.FilePath}({d.LineNumber}): {d.Message}")) +
                         $"\nTotal: {results.Count} unused private fields.";

            return CommandResult.Ok(output);
        }
    }
}