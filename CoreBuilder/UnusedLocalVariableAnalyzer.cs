/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        UnusedLocalVariableAnalyzer.cs
 * PURPOSE:     Unused local variable Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder;

/// <summary>
/// Analyzer that finds unused local variables.
/// </summary>
public sealed class UnusedLocalVariableAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "UnusedLocalVariable";

    /// <inheritdoc />
    public string Description => "Unused local variable Analyzer.";

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

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
                var symbol = model.GetDeclaredSymbol(variable);
                if (symbol is ILocalSymbol localSymbol)
                {
                    var references = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id =>
                            SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, localSymbol));

                    if (!references.Any())
                    {
                        var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                            $"Unused local variable '{variable.Identifier.Text}'.");
                    }
                }
            }
        }
    }
}
