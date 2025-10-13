/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/DisposableAnalyzer.cs
 * PURPOSE:     Analyzer that detects undisposed IDisposable objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules;

/// <summary>
/// Analyzer that detects undisposed IDisposable objects.
/// </summary>
/// <seealso cref="ICodeAnalyzer" />
public sealed class DisposableAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "DisposableLeak";

    /// <inheritdoc />
    public string Description => "Analyzer that detects undisposed IDisposable objects.";

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        var declarations = root.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Where(v => v.Type is IdentifierNameSyntax id && ImplementsIDisposable(id.Identifier.Text));

        foreach (var decl in declarations)
        {
            foreach (var v in decl.Variables)
            {
                // check if inside using statement
                if (!IsDisposed(v, root))
                {
                    var line = v.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Warning,
                        filePath,
                        line,
                        $"'{v.Identifier.Text}' implements IDisposable but is not disposed. Risk of resource leak.",
                        DiagnosticImpact.IoBound
                    );
                }
            }
        }
    }

    //
    /// <summary>
    ///  Dummy check for demonstration; could be extended with semantic model
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>
    ///   <c>true</c> if the specified resource is disposed; otherwise, <c>false</c>.
    /// </returns>
    private static bool ImplementsIDisposable(string typeName)
    {
        return typeName.EndsWith("Stream") || typeName.EndsWith("Reader") || typeName.EndsWith("Writer");
    }

    /// <summary>
    /// Determines whether the specified variable is disposed.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="root">The root.</param>
    /// <returns>
    ///   <c>true</c> if the specified variable is disposed; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsDisposed(VariableDeclaratorSyntax variable, SyntaxNode root)
    {
        // Simplified: check for using block
        var usingStatements = root.DescendantNodes().OfType<UsingStatementSyntax>();
        foreach (var u in usingStatements)
        {
            if (u.Declaration?.Variables.Any(v => v.Identifier.Text == variable.Identifier.Text) ?? false)
                return true;
        }

        return false;
    }
}
