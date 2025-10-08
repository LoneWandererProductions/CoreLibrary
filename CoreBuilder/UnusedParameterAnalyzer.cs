/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        UnusedParameterAnalyzer.cs
 * PURPOSE:     Unused parameter Analyzer.
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
/// Analyzer that finds unused method parameters.
/// </summary>
public sealed class UnusedParameterAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "UnusedParameter";

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

        foreach (var methodDecl in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            // Skip abstract/interface methods (no body to analyze)
            if (methodDecl.Body == null && methodDecl.ExpressionBody == null)
                continue;

            foreach (var parameter in methodDecl.ParameterList.Parameters)
            {
                var symbol = model.GetDeclaredSymbol(parameter);
                if (symbol is IParameterSymbol paramSymbol)
                {
                    var references = methodDecl.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, paramSymbol));

                    if (!references.Any())
                    {
                        var line = parameter.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(Name, DiagnosticSeverity.Warning, filePath, line,
                            $"Unused parameter '{parameter.Identifier.Text}' in method '{methodDecl.Identifier.Text}'.");
                    }
                }
            }
        }
    }
}
