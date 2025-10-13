/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/FieldNamingConventionAnalyzer.cs
 * PURPOSE:     Ensures that field names follow consistent naming conventions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules
{
    /// <summary>
    /// Analyzer that enforces field naming conventions depending on visibility and constness.
    /// </summary>
    public sealed class FieldNamingConventionAnalyzer : ICodeAnalyzer
    {
        /// <inheritdoc />
        public string Name => nameof(FieldNamingConventionAnalyzer);

        /// <inheritdoc />
        public string Description =>
            "Ensures field names follow proper casing conventions based on visibility and constness.";

        // Regex patterns for naming rules

        /// <summary>
        /// The private field pattern
        /// </summary>
        private static readonly Regex PrivateFieldPattern = new(@"^_[a-z][a-zA-Z0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// The public field pattern
        /// </summary>
        private static readonly Regex PublicFieldPattern = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// The constant field pattern
        /// </summary>
        private static readonly Regex ConstantFieldPattern = new(@"^[A-Z0-9_]+$", RegexOptions.Compiled);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            // 🔹 Ignore generated code and compiler artifacts
            if (CoreHelper.ShouldIgnoreFile(filePath))
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                yield break;
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            // 🔹 Analyze field declarations
            foreach (var fieldDecl in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                var modifiers = fieldDecl.Modifiers;
                var isConst = modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
                var isPublic = modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
                var isPrivate = modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) || !isPublic;

                foreach (var variable in fieldDecl.Declaration.Variables)
                {
                    var name = variable.Identifier.Text;

                    // Skip compiler-generated fields like <Foo>k__BackingField
                    if (name.Contains("<") || name.Contains(">"))
                        continue;

                    var lineNumber = GetLineNumber(fieldDecl);

                    if (isConst)
                    {
                        if (!ConstantFieldPattern.IsMatch(name))
                        {
                            yield return new Diagnostic(
                                Name,
                                DiagnosticSeverity.Info,
                                filePath,
                                lineNumber,
                                $"Constant '{name}' should be in UPPER_CASE_WITH_UNDERSCORES format."
                            );
                        }
                    }
                    else if (isPrivate)
                    {
                        if (!PrivateFieldPattern.IsMatch(name))
                        {
                            yield return new Diagnostic(
                                Name,
                                DiagnosticSeverity.Info,
                                filePath,
                                lineNumber,
                                $"Private field '{name}' should start with '_' and use camelCase (e.g., '_myField')."
                            );
                        }
                    }
                    else if (isPublic)
                    {
                        if (!PublicFieldPattern.IsMatch(name))
                        {
                            yield return new Diagnostic(
                                Name,
                                DiagnosticSeverity.Info,
                                filePath,
                                lineNumber,
                                $"Public field '{name}' should be PascalCase (e.g., 'MyField')."
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the 1-based line number of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The line number.</returns>
        private static int GetLineNumber(SyntaxNode node)
        {
            try
            {
                return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            }
            catch
            {
                return 1;
            }
        }
    }
}
