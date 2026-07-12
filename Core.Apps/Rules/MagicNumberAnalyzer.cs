/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        MagicNumberAnalyzer.cs
 * PURPOSE:     Detects unexplained numeric literals (magic numbers).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Apps.Enums;
using Core.Apps.Helper;
using Core.Apps.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects unexplained numeric literals (magic numbers) in method bodies, which can hurt readability and maintainability.
    /// It ignores common "safe" numbers like 0, 1, -1, and 2, as well as literals that are part of constant definitions.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class MagicNumberAnalyzer : ICommand, ICodeAnalyzer
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "Analyzer";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "MagicNumber";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects unexplained numeric literals in method bodies.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// The safe numbers
        /// Constants that are usually considered "safe" or self-documenting
        /// </summary>
        private static readonly HashSet<string> SafeNumbers = new() { "0", "1", "-1", "2" };

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath)) yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var root = tree.GetCompilationUnitRoot();

            // Find all methods
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                // Find all numeric literals inside the method body
                var literals = method.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .Where(l => l.IsKind(SyntaxKind.NumericLiteralExpression));

                foreach (var literal in literals)
                {
                    string value = literal.Token.ValueText;

                    // Skip "safe" numbers and ignore if it's already part of a constant definition
                    if (SafeNumbers.Contains(value) || IsInConstantDefinition(literal))
                        continue;

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"Magic number '{value}' detected. Replace with a named constant.",
                        DiagnosticImpact.Readability
                    );
                }
            }
        }

        /// <summary>
        /// Determines whether [is in constant definition] [the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if [is in constant definition] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsInConstantDefinition(SyntaxNode node)
        {
            // Don't flag if the literal is actually being assigned to a 'const' variable
            return node.Ancestors().OfType<FieldDeclarationSyntax>().Any(f => f.Modifiers.Any(SyntaxKind.ConstKeyword));
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: MagicNumber <fileOrDirectoryPath>");
                var output = results.Count > 0
                    ? string.Join("\n",
                        results.Select(d => $"{Path.GetFileName(d.FilePath)} ({d.LineNumber}): {d.Message}"))
                    : "No magic numbers found.";
                return CommandResult.Ok(output, EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
