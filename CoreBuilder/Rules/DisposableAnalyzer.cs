/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DisposableAnalyzer.cs
 * PURPOSE:     Analyzer that detects undisposed IDisposable objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
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

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Analyzer that detects undisposed IDisposable objects.
/// </summary>
/// <seealso cref="ICodeAnalyzer" />
public sealed class DisposableAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "DisposableLeak";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer that detects undisposed IDisposable objects.";

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
                        Enums.DiagnosticSeverity.Warning,
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

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        List<Diagnostic> results;
        try
        {
            results = AnalyzerExecutor.ExecutePath(this, args, "Usage: DisposableLeak<fileOrDirectoryPath>");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(ex.Message);
        }

        var output = string.Join("\n", results.Select(d => d.ToString()));
        return CommandResult.Ok(output, results);
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }
}