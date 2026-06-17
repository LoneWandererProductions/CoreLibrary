/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        StructPaddingAnalyzer.cs
 * PURPOSE:     Analyzes struct field ordering to minimize memory padding.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
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
    /// Analyzer that detects inefficient struct field ordering that causes unnecessary memory padding.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class StructPaddingAnalyzer : ICodeAnalyzer, ICommand
    {

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "StructPadding";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Analyzes struct field ordering to optimize memory alignment.";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath)) yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetCompilationUnitRoot();

            foreach (var structDecl in root.DescendantNodes().OfType<StructDeclarationSyntax>())
            {
                var fields = structDecl.Members.OfType<FieldDeclarationSyntax>();
                var fieldList = fields.SelectMany(f => f.Declaration.Variables.Select(v => new {
                    Name = v.Identifier.Text,
                    Size = GetFieldSize(f.Declaration.Type.ToString())
                })).ToList();

                if (fieldList.Count <= 1) continue;

                var optimalOrder = fieldList.OrderByDescending(f => f.Size).ToList();

                // Check if current matches optimal
                bool isOptimal = true;
                for (int i = 0; i < fieldList.Count; i++)
                {
                    if (fieldList[i].Name != optimalOrder[i].Name)
                    {
                        isOptimal = false;
                        break;
                    }
                }

                if (!isOptimal)
                {
                    var line = structDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    string suggestion = string.Join(", ", optimalOrder.Select(f => f.Name));

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Struct '{structDecl.Identifier.Text}' is not optimized. " +
                        $"Reorder fields to: {suggestion}",
                        DiagnosticImpact.Other
                    );
                }
            }
        }

        /// <summary>
        /// Gets the size of the field.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The size of the type in bytes.</returns>
        private static int GetFieldSize(string typeName) => typeName switch
        {
            "long" or "double" => 8,
            "int" or "float" => 4,
            "short" or "char" => 2,
            "bool" or "byte" => 1,
            _ => 8 // Conservative default
        };

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: StructPadding <fileOrDirectoryPath>");
                var output = results.Count > 0
                    ? string.Join("\n", results.Select(d => d.ToString()))
                    : "No padding issues found.";

                return CommandResult.Ok(output, EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
