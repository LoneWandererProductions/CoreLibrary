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
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath)) yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetCompilationUnitRoot();

            // Find all executable code blocks: Methods, Constructors, Destructors, Accessors (get/set), and Indexers
            var codeBlocks = root.DescendantNodes().Where(n =>
                n is MethodDeclarationSyntax ||
                n is ConstructorDeclarationSyntax ||
                n is AccessorDeclarationSyntax ||
                n is IndexerDeclarationSyntax);

            foreach (var block in codeBlocks)
            {
                // Find all numeric literals inside this code block
                var literals = block.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .Where(l => l.IsKind(SyntaxKind.NumericLiteralExpression));

                foreach (var literal in literals)
                {
                    var value = literal.Token.ValueText;

                    // Skip safe numbers and any initialization contexts
                    if (SafeNumbers.Contains(value) || IsInSafeContext(literal))
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
        /// Determines whether the literal is part of a constant definition, property initializer, or field initializer.
        /// </summary>
        private static bool IsInSafeContext(SyntaxNode node)
        {
            // Ignore if it's part of a const field
            if (node.Ancestors().OfType<FieldDeclarationSyntax>().Any(f => f.Modifiers.Any(SyntaxKind.ConstKeyword)))
                return true;

            // Ignore if it's an auto-property initializer (e.g. public int Value { get; set; } = 52;)
            if (node.Ancestors().OfType<EqualsValueClauseSyntax>().Any(e => e.Parent is PropertyDeclarationSyntax))
                return true;

            // Ignore if it's a field initializer (e.g. private int _val = 52;)
            if (node.Ancestors().OfType<EqualsValueClauseSyntax>().Any(e => e.Parent is VariableDeclaratorSyntax))
                return true;

            return false;
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
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
