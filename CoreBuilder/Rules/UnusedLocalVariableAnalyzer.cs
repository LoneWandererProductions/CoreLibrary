/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        UnusedLocalVariableAnalyzer.cs
 * PURPOSE:     Unused local variable Analyzer.
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
    /// Analyzer that finds unused local variables.
    /// </summary>
    public sealed class UnusedLocalVariableAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedLocalVariable";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Unused local variable Analyzer.";

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

            foreach (var localDecl in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    if (variable.Identifier.Text == "_")
                        continue; // discard, don’t flag

                    var symbol = model.GetDeclaredSymbol(variable);
                    if (symbol is not ILocalSymbol localSymbol)
                        continue;

                    var references = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id =>
                            SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, localSymbol));

                    if (references.Any())
                        continue;

                    var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                        $"Unused local variable '{variable.Identifier.Text}'.");
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedLocalVariable <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var msg = string.Join(Environment.NewLine,
                results.Select(d => $"{d.FilePath}:{d.LineNumber} -> {d.Message}")
            );

            return CommandResult.Ok(msg);
        }
    }
}
