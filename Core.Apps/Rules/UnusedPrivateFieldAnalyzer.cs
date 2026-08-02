/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedPrivateFieldAnalyzer.cs
 * PURPOSE:     Unused private field Analyzer.
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
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
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

            // Find all field declarations
            foreach (var fieldDecl in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                // Process each variable declared in the field (e.g., private int _used, _unused;)
                foreach (var variable in fieldDecl.Declaration.Variables)
                {
                    var symbol = model.GetDeclaredSymbol(variable);
                    if (symbol is not IFieldSymbol fieldSymbol)
                        continue;

                    // 1. Rely on Semantic Model for accessibility (catches implicit private)
                    if (fieldSymbol.DeclaredAccessibility != Microsoft.CodeAnalysis.Accessibility.Private)
                        continue;

                    // 2. Use your robust CoreHelper check instead of a manual IdentifierNameSyntax search
                    if (!CoreHelper.IsSymbolUsed(model, root, fieldSymbol))
                    {
                        var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Unused private field '{variable.Identifier.Text}'.");
                    }
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
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