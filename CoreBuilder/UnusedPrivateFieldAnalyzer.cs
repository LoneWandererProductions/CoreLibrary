/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        UnusedPrivateFieldAnalyzer.cs
 * PURPOSE:     Unused private field Analyzer.
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
/// Analyzer that finds unused private fields.
/// </summary>
public sealed class UnusedPrivateFieldAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "UnusedPrivateField";

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

        foreach (var fieldDecl in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            // Only look at private fields
            if (!fieldDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                continue;

            foreach (var variable in fieldDecl.Declaration.Variables)
            {
                var symbol = model.GetDeclaredSymbol(variable);
                if (symbol is IFieldSymbol fieldSymbol)
                {
                    var references = root.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Where(id =>
                            SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, fieldSymbol));

                    if (!references.Any())
                    {
                        var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, line,
                            $"Unused private field '{variable.Identifier.Text}'.");
                    }
                }
            }
        }
    }
}
