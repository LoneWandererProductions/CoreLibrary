/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        UnusedMemberAnalyzer.cs
 * PURPOSE:     Detects unused private fields, methods, and constants in a file.
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
    /// Analyzer that detects unused private fields, methods, and constants in a file. It uses the Roslyn API to parse the C# code, identify private members, and check if they are referenced anywhere in the file. If a private member is found to be unused, it reports a diagnostic message indicating the member's name and location.
    /// This helps developers identify and clean up dead code, improving readability and maintainability.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class UnusedMemberAnalyzer : ICommand, ICodeAnalyzer
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "Analyzer";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "UnusedMember";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects unused private fields, methods, and constants in a file.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath)) yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetCompilationUnitRoot();

            // We will gather all symbols we care about into a single list to process uniformly.
            // Storing the Symbol, the SyntaxNode for location, and the Name.
            var symbolsToCheck = new List<(ISymbol Symbol, SyntaxNode Node, string Name)>();

            // 1. Regular Members (Methods, Properties, Classes, etc.)
            // We exclude FieldDeclarationSyntax here because a single field declaration can
            // declare multiple variables, which we handle in step 2.
            var memberDecls = root.DescendantNodes()
                .OfType<MemberDeclarationSyntax>()
                .Where(m => m is not FieldDeclarationSyntax);

            foreach (var member in memberDecls)
            {
                var symbol = model.GetDeclaredSymbol(member);
                if (symbol != null)
                    symbolsToCheck.Add((symbol, member, CoreHelper.GetMemberName(member)));
            }

            // 2. Variables (Both Class-level Fields AND Method-level Local Variables)
            var variableDecls = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();

            foreach (var variable in variableDecls)
            {
                var symbol = model.GetDeclaredSymbol(variable);
                if (symbol != null)
                    symbolsToCheck.Add((symbol, variable, variable.Identifier.Text));
            }

            // 3. Analyze all gathered symbols
            foreach (var (symbol, node, name) in symbolsToCheck)
            {
                var isLocalVariable = symbol is ILocalSymbol;
                var isPrivateMember = symbol.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private;

                // We only care about Private class members OR Local variables
                if (!isPrivateMember && !isLocalVariable)
                    continue;

                // Skip discards (e.g., `_ = DoSomething()`) and compiler-generated locals
                if (isLocalVariable && (name == "_" || symbol.IsImplicitlyDeclared))
                    continue;

                // 4. Check for usage
                if (!CoreHelper.IsSymbolUsed(model, root, symbol))
                {
                    var symbolType = isLocalVariable ? "Local variable" : "Private member";

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"{symbolType} '{name}' is never used within this file.",
                        DiagnosticImpact.Readability
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: UnusedMember <fileOrDirectoryPath>");

                if (results.Count == 0) return CommandResult.Ok("No unused private members found.");

                var output = string.Join("\n",
                    results.Select(d => $"{Path.GetFileName(d.FilePath)} ({d.LineNumber}): {d.Message}"));
                return CommandResult.Ok($"Unused members detected:\n{output}", EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
