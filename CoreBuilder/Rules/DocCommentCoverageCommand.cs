/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DocCommentCoverageCommand.cs
 * PURPOSE:     Checks code for XML documentation comment coverage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Command that checks code for XML documentation comment coverage.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class DocCommentCoverageCommand : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "analysis";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "doccoverage";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Reports the percentage of public members with XML doc comments.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                // Check type-level doc comments
                var trivia = typeDecl.GetLeadingTrivia();
                var hasXmlDoc = CoreHelper.HasXmlDocTrivia(trivia);

                if (!hasXmlDoc)
                {
                    var line = typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Type '{CoreHelper.GetTypeFullName(typeDecl)}' is missing XML documentation.",
                        DiagnosticImpact.Readability
                    );
                }

                // Check members
                foreach (var member in typeDecl.Members)
                {
                    var memberTrivia = member.GetLeadingTrivia();
                    var memberHasDoc = CoreHelper.HasXmlDocTrivia(memberTrivia);

                    if (!memberHasDoc)
                    {
                        var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Member '{CoreHelper.GetMemberName(member)}' is missing XML documentation.",
                            DiagnosticImpact.Readability
                        );
                    }
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
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
            return CommandResult.Ok(messages, results);
        }


        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}