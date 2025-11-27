/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        ApiExplorerCommand.cs
 * PURPOSE:     Command to list namespaces, classes, and members in a source directory.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CoreBuilder.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Command to explore C# source structure (namespaces, classes, members, interfaces, etc.).
    /// </summary>
    public sealed class ApiExplorerCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "apiexplore";

        /// <inheritdoc />
        public string Description =>
            "Scans source files and lists namespaces, classes, interfaces, and public members.";

        /// <inheritdoc />
        public string Namespace => "Development";

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage: apiexplore <folder>");

            var rootPath = args[0];
            if (!Directory.Exists(rootPath))
                return CommandResult.Fail($"Folder not found: {rootPath}");

            var sb = new StringBuilder();
            var files = Directory
                .EnumerateFiles(rootPath, CoreResources.ResourceCsExtension, SearchOption.AllDirectories)
                .Where(f => !CoreHelper.ShouldIgnoreFile(f)); // <-- filter out ignored files

            foreach (var file in files)
            {
                try
                {
                    var code = File.ReadAllText(file);
                    var tree = CSharpSyntaxTree.ParseText(code);
                    var root = tree.GetCompilationUnitRoot();

                    // handle both block-style and file-scoped namespaces
                    var namespaces = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
                    if (!namespaces.Any())
                    {
                        // handle top-level types (no namespace)
                        DumpTypes(root.Members.OfType<BaseTypeDeclarationSyntax>(), sb, "(global)");
                    }
                    else
                    {
                        foreach (var ns in namespaces)
                            DumpTypes(ns.Members.OfType<BaseTypeDeclarationSyntax>(), sb, ns.Name.ToString());
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"// Error parsing {file}: {ex.Message}");
                }
            }

            return CommandResult.Ok(sb.ToString());
        }

        /// <summary>
        /// Writes all type declarations and their members to the StringBuilder.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="sb">The sb.</param>
        /// <param name="nsName">Name of the ns.</param>
        private static void DumpTypes(IEnumerable<BaseTypeDeclarationSyntax> types, StringBuilder sb, string nsName)
        {
            // Filter to only public types
            var publicTypes = types.Where(t => IsPublic(t.Modifiers)).ToList();
            if (publicTypes.Count == 0)
                return; // Nothing public => skip the whole namespace

            sb.AppendLine($"namespace {nsName}");
            foreach (var type in publicTypes)
            {
                var modifiers = string.Join(" ", type.Modifiers);
                var typeKind = type switch
                {
                    ClassDeclarationSyntax => "class",
                    StructDeclarationSyntax => "struct",
                    InterfaceDeclarationSyntax => "interface",
                    EnumDeclarationSyntax => "enum",
                    _ => "type"
                };

                var baseList = (type as TypeDeclarationSyntax)?.BaseList;
                var bases = baseList is { Types.Count: > 0 }
                    ? $" : {string.Join(", ", baseList.Types.Select(b => b.Type.ToString()))}"
                    : string.Empty;

                sb.AppendLine($"  {modifiers} {typeKind} {type.Identifier}{bases}");

                if (type is TypeDeclarationSyntax typeDecl)
                {
                    foreach (var member in typeDecl.Members)
                    {
                        switch (member)
                        {
                            case MethodDeclarationSyntax m when IsPublic(m.Modifiers):
                                var isExtension = m.ParameterList.Parameters.FirstOrDefault()?
                                    .Modifiers.Any(mod => mod.IsKind(SyntaxKind.ThisKeyword)) == true;

                                var prefix = isExtension ? "extension method" : "method";
                                sb.AppendLine(
                                    $"    {prefix}: {m.Identifier}({string.Join(", ", m.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})");
                                break;

                            case PropertyDeclarationSyntax p when IsPublic(p.Modifiers):
                                sb.AppendLine($"    property: {p.Identifier} : {p.Type}");
                                break;

                            case FieldDeclarationSyntax f when IsPublic(f.Modifiers):
                                sb.AppendLine(
                                    $"    field: {string.Join(", ", f.Declaration.Variables.Select(v => v.Identifier.Text))} : {f.Declaration.Type}");
                                break;

                            case EventDeclarationSyntax e when IsPublic(e.Modifiers):
                                sb.AppendLine($"    event: {e.Identifier} : {e.Type}");
                                break;
                        }
                    }
                }

                sb.AppendLine();
            }
        }

        /// <summary>
        /// Checks if a syntax token list contains a "public" modifier.
        /// </summary>
        /// <param name="modifiers">The modifiers.</param>
        /// <returns>
        ///   <c>true</c> if the specified modifiers is public; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPublic(SyntaxTokenList modifiers)
        {
            return modifiers.Any(m => m.Text == "public");
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}