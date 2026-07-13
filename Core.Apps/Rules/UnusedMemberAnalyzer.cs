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
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath)) yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            // 1. Setup Semantic Model
            // We add the standard object reference so the compiler knows about basic types
            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetCompilationUnitRoot();

            // 2. Identify all private members
            var privateMembers = root.DescendantNodes()
                .OfType<MemberDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PrivateKeyword)));

            foreach (var member in privateMembers)
            {
                // 3. Get the symbol for the private member using the SemanticModel
                var symbol = model.GetDeclaredSymbol(member);

                // Skip members where we couldn't resolve a symbol (e.g., complex syntax)
                if (symbol == null) continue;

                // 4. Use your CoreHelper to check if the symbol is used anywhere in the file
                if (!CoreHelper.IsSymbolUsed(model, root, symbol))
                {
                    // We need a name for the diagnostic message
                    var memberName = CoreHelper.GetMemberName(member);

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        member.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"Private member '{memberName}' is never used within this file.",
                        DiagnosticImpact.Readability
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
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
