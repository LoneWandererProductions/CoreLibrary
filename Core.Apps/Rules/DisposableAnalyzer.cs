/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        DisposableAnalyzer.cs
 * PURPOSE:     Analyzer that detects undisposed IDisposable objects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

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
    /// Analyzer that detects undisposed IDisposable objects.
    /// </summary>
    public sealed class DisposableAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "DisposableLeak";

        /// <inheritdoc cref="ICommand" />
        public string Description => "Analyzer that detects undisposed IDisposable objects.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
            {
                yield break;
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            // Find all individual variables declared in the file
            var variables = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();

            foreach (var v in variables)
            {
                if (!IsDisposableType(v) || IsProperlyDisposed(v)) continue;

                var line = v.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Warning,
                    filePath,
                    line,
                    $"'{v.Identifier.Text}' is a disposable type but is not safely disposed. Use a 'using' statement or call .Dispose().",
                    DiagnosticImpact.IoBound
                );
            }
        }

        /// <summary>
        /// Checks if the variable is recognized as a disposable type, even if 'var' is used.
        /// </summary>
        private static bool IsDisposableType(VariableDeclaratorSyntax variable)
        {
            // 1. Check explicit type declarations (e.g., StreamReader reader = ...)
            if (variable.Parent is VariableDeclarationSyntax decl)
            {
                var typeName = decl.Type.ToString();
                if (typeName != "var" && ImplementsIDisposable(typeName))
                    return true;
            }

            // 2. Check implicit 'var' declarations by looking at the right side of the assignment (e.g., var reader = new StreamReader(...))
            if (variable.Initializer?.Value is ObjectCreationExpressionSyntax objCreate)
            {
                var typeName = objCreate.Type.ToString();
                if (ImplementsIDisposable(typeName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Dummy check for demonstration; could be extended with a semantic model.
        /// </summary>
        private static bool ImplementsIDisposable(string typeName)
        {
            return typeName.EndsWith("Stream") ||
                   typeName.EndsWith("Reader") ||
                   typeName.EndsWith("Writer");
        }

        /// <summary>
        /// Determines whether the specified variable is disposed safely.
        /// </summary>
        private static bool IsProperlyDisposed(VariableDeclaratorSyntax variable)
        {
            // 1. Check for traditional using block: using (var stream = new ...) { }
            if (variable.Parent?.Parent is UsingStatementSyntax)
                return true;

            // 2. Check for modern C# 8 using declaration: using var stream = new ...;
            if (variable.Parent?.Parent is LocalDeclarationStatementSyntax localDecl &&
                localDecl.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                return true;

            // 3. Check if .Dispose() is manually called on this variable within its parent method block
            var parentMethod = variable.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (parentMethod != null)
            {
                var isDisposedManually = parentMethod.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                                memberAccess.Expression.ToString() == variable.Identifier.Text &&
                                memberAccess.Name.Identifier.Text == "Dispose");

                if (isDisposedManually)
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
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: DisposableLeak <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var output = string.Join("\n", results.Select(d => d.ToString()));
            return CommandResult.Ok(output, EnumTypes.Wstring);
        }
    }
}
