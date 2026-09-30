/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        DocCommentCoverageCommand.cs
 * PURPOSE:     Checks code for XML documentation comment coverage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

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
    /// Command that checks code for XML documentation comment coverage.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class DocCommentCoverageCommand : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "Analyzer";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "doccoverage";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Reports missing XML doc comments or missing <inheritdoc /> tags on types, methods, properties, and attributes.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            // 1. Check Type Declarations (classes, structs, interfaces, enums, record types)
            foreach (var typeDecl in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var trivia = typeDecl.GetLeadingTrivia();
                if (!HasValidDocumentation(trivia))
                {
                    var line = typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Type '{typeDecl.Identifier.Text}' is missing XML documentation or '<inheritdoc />'.",
                        DiagnosticImpact.Readability
                    );
                }

                // Retrieve members safely based on the specific declaration type
                IEnumerable<MemberDeclarationSyntax> members = typeDecl switch
                {
                    TypeDeclarationSyntax t => t.Members,
                    EnumDeclarationSyntax e => e.Members,
                    _ => Enumerable.Empty<MemberDeclarationSyntax>()
                };

                // 2. Check Type Members (methods, properties, fields, events, constructors, indexers, enum members)
                foreach (var member in members)
                {
                    var memberTrivia = member.GetLeadingTrivia();
                    if (HasValidDocumentation(memberTrivia))
                        continue;

                    var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    var memberName = CoreHelper.GetMemberName(member);

                    // Provide specific guidance if the member is an override or explicit interface implementation
                    if (isOverrideOrInterfaceImplementation(member))
                    {
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Member '{memberName}' is an override or interface implementation but is missing '<inheritdoc />' or XML documentation.",
                            DiagnosticImpact.Readability
                        );
                    }
                    else
                    {
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Member '{memberName}' is missing XML documentation.",
                            DiagnosticImpact.Readability
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the leading trivia contains valid XML documentation comments or an &lt;inheritdoc /&gt; tag.
        /// </summary>
        private static bool HasValidDocumentation(SyntaxTriviaList trivia)
        {
            foreach (var t in trivia)
            {
                if (t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    var commentText = t.ToFullString();

                    // If XML comments exist OR <inheritdoc> is present, count as documented
                    if (!string.IsNullOrWhiteSpace(commentText))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a member declaration is an override or explicit interface implementation.
        /// </summary>
        private static bool isOverrideOrInterfaceImplementation(MemberDeclarationSyntax member)
        {
            return member switch
            {
                MethodDeclarationSyntax m => m.Modifiers.Any(SyntaxKind.OverrideKeyword) || m.ExplicitInterfaceSpecifier != null,
                PropertyDeclarationSyntax p => p.Modifiers.Any(SyntaxKind.OverrideKeyword) || p.ExplicitInterfaceSpecifier != null,
                EventDeclarationSyntax e => e.Modifiers.Any(SyntaxKind.OverrideKeyword) || e.ExplicitInterfaceSpecifier != null,
                _ => false
            };
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: Doccoverage <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var messages = string.Join("\n", results.Select(d => d.ToString()));
            return CommandResult.Ok(messages, EnumTypes.Wstring);
        }
    }
}