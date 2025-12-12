/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Development
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
using CoreBuilder.Interface;
using CoreBuilder.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Development
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
        /// <summary>
        ///  folder + optional outputMode
        /// </summary>
        public int ParameterCount => 2;

        /// <summary>
        /// The output
        /// </summary>
        private readonly IEventOutput _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExplorerCommand"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public ApiExplorerCommand(IEventOutput? output = null)
        {
            _output = output ?? new WpfEventOutput();
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length == 0)
                return CommandResult.Fail("Usage: apiexplore <folder>");

            var rootPath = args[0];
            if (!Directory.Exists(rootPath))
                return CommandResult.Fail($"Folder not found: {rootPath}");

            bool useWindow = args.Length > 1 && args[1].Equals("window", StringComparison.OrdinalIgnoreCase);

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

                    var namespaces = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
                    if (!namespaces.Any())
                        DumpTypes(root.Members.OfType<BaseTypeDeclarationSyntax>(), sb, _output, "(global)");
                    else
                        foreach (var ns in namespaces)
                            DumpTypes(ns.Members.OfType<BaseTypeDeclarationSyntax>(), sb, _output, ns.Name.ToString());
                }
                catch (Exception ex)
                {
                    var line = $"// Error parsing {file}: {ex.Message}";
                    sb.AppendLine(line);
                    _output?.Write(line);
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
        private static void DumpTypes(IEnumerable<BaseTypeDeclarationSyntax> types, StringBuilder sb,
            IEventOutput? output, string nsName)
        {
            var publicTypes = types.Where(t => IsPublic(t.Modifiers)).ToList();
            if (publicTypes.Count == 0) return;

            var nsLine = $"namespace {nsName}";
            sb.AppendLine(nsLine);
            output?.Write(nsLine);

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

                var typeLine = $"  {modifiers} {typeKind} {type.Identifier}{bases}";
                sb.AppendLine(typeLine);
                output?.Write(typeLine);

                if (type is TypeDeclarationSyntax typeDecl)
                {
                    foreach (var member in typeDecl.Members)
                    {
                        string? memberLine = member switch
                        {
                            MethodDeclarationSyntax m when IsPublic(m.Modifiers) =>
                                $"    method: {m.Identifier}({string.Join(", ", m.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})",
                            PropertyDeclarationSyntax p when IsPublic(p.Modifiers) =>
                                $"    property: {p.Identifier} : {p.Type}",
                            FieldDeclarationSyntax f when IsPublic(f.Modifiers) =>
                                $"    field: {string.Join(", ", f.Declaration.Variables.Select(v => v.Identifier.Text))} : {f.Declaration.Type}",
                            EventDeclarationSyntax e when IsPublic(e.Modifiers) =>
                                $"    event: {e.Identifier} : {e.Type}",
                            _ => null
                        };

                        if (memberLine != null)
                        {
                            sb.AppendLine(memberLine);
                            output?.Write(memberLine);
                        }
                    }
                }

                sb.AppendLine();
                output?.Write("");
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
