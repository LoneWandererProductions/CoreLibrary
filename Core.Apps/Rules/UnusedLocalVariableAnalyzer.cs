/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedLocalVariableAnalyzer.cs
 * PURPOSE:     Unused local variable Analyzer.
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

            // 1. Standard Local Declaration Statements (var x = ...; using var stream = ...;)
            foreach (var localDecl in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                foreach (var variable in localDecl.Declaration.Variables)
                {
                    if (variable.Identifier.Text == "_")
                        continue; // discard, don’t flag

                    var symbol = model.GetDeclaredSymbol(variable);
                    if (symbol is not ILocalSymbol localSymbol)
                        continue;

                    if (CoreHelper.IsSymbolUsed(model, root, localSymbol))
                        continue;

                    var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                        $"Unused local variable '{variable.Identifier.Text}'.");
                }
            }

            // 2. Pattern Matching and Out-Variable Declarations (e.g., int.TryParse(s, out var x), if (obj is MyType x))
            foreach (var designation in root.DescendantNodes().OfType<SingleVariableDesignationSyntax>())
            {
                var varName = designation.Identifier.Text;
                if (varName == "_")
                    continue;

                var symbol = model.GetDeclaredSymbol(designation);
                if (symbol is not ILocalSymbol localSymbol)
                    continue;

                // Skip if parent is a standard local declaration (already processed above)
                if (designation.Ancestors().OfType<LocalDeclarationStatementSyntax>().Any())
                    continue;

                if (!CoreHelper.IsSymbolUsed(model, root, localSymbol))
                {
                    var line = designation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                        $"Unused local variable '{varName}'.");
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
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
