/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        CoreHelper.cs
 * PURPOSE:     Common helper methods shared by analyzers, extractors, and console tools.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CoreBuilder.Helper;

/// <summary>
/// Provides reusable utility methods for CoreBuilder tools.
/// </summary>
internal static class CoreHelper
{
    /// <summary>
    /// The ignore cache
    /// </summary>
    private static readonly Dictionary<string, bool> IgnoreCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The source file cache
    /// </summary>
    private static readonly Dictionary<string, string[]> SourceFileCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether a given file should be ignored during analysis.
    /// </summary>
    /// <param name="filePath">The absolute path of the file.</param>
    /// <returns>
    /// <see langword="true"/> if the file is auto-generated or excluded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool ShouldIgnoreFile(string filePath)
    {
        if (IgnoreCache.TryGetValue(filePath, out var cached))
            return cached;

        var result = ShouldIgnore(filePath);
        IgnoreCache[filePath] = result;
        return result;
    }

    /// <summary>
    /// Determines the loop context (constant, variable, or nested)
    /// for a given syntax node.
    /// </summary>
    /// <param name="node">The syntax node to analyze.</param>
    /// <returns>
    /// A <see cref="LoopContext"/> value indicating the loop's classification.
    /// </returns>
    internal static LoopContext GetLoopContext(SyntaxNode node)
    {
        var loops = node.Ancestors().Where(a =>
            a is ForStatementSyntax ||
            a is ForEachStatementSyntax ||
            a is WhileStatementSyntax ||
            a is DoStatementSyntax).ToList();

        if (!loops.Any())
            return LoopContext.None;

        if (loops.Count > 1)
            return LoopContext.Nested;

        var loop = loops.First();
        return loop switch
        {
            ForStatementSyntax forLoop => AnalyzeForLoop(forLoop),
            ForEachStatementSyntax => LoopContext.VariableBounded,
            WhileStatementSyntax => LoopContext.VariableBounded,
            DoStatementSyntax => LoopContext.VariableBounded,
            _ => LoopContext.VariableBounded
        };
    }

    /// <summary>
    /// Finds the root of a project by locating the nearest .csproj or directory with source files.
    /// </summary>
    /// <param name="startPath">The path to start searching from.</param>
    /// <returns>The project root directory.</returns>
    internal static string FindProjectRoot(string startPath)
    {
        // Normalize: convert file path → directory path
        if (File.Exists(startPath))
            startPath = Path.GetDirectoryName(startPath)!;

        var dir = new DirectoryInfo(startPath);

        while (dir != null)
        {
            if (dir.GetFiles(CoreResources.ResourceCsProjectExtension).Any())
                return dir.FullName;

            dir = dir.Parent;
        }

        // Fallback
        return startPath;
    }

    /// <summary>
    /// Checks whether the given trivia list contains XML documentation comments.
    /// </summary>
    /// <param name="trivia">The trivia list to inspect.</param>
    /// <returns><c>true</c> if XML documentation exists; otherwise <c>false</c>.</returns>
    internal static bool HasXmlDocTrivia(SyntaxTriviaList trivia)
    {
        // XML documentation comments appear as SingleLineDocumentationCommentTrivia
        // or MultiLineDocumentationCommentTrivia.
        foreach (var t in trivia)
        {
            if (t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>Name of Member.</returns>
    internal static string GetMemberName(MemberDeclarationSyntax member)
    {
        return member switch
        {
            MethodDeclarationSyntax m => m.Identifier.Text,
            PropertyDeclarationSyntax p => p.Identifier.Text,
            FieldDeclarationSyntax f => string.Join(", ", f.Declaration.Variables.Select(v => v.Identifier.Text)),
            EventDeclarationSyntax e => e.Identifier.Text,
            _ => member.Kind().ToString()
        };
    }

    /// <summary>
    /// Recursively enumerates files while skipping inaccessible folders.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>All Folders.</returns>
    internal static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
    {
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            var files = Array.Empty<string>();
            try
            {
                files = Directory.GetFiles(current, pattern);
            }
            catch (Exception exe)
            {
                Trace.WriteLine($"Error accessing files in directory {current}: {exe.Message}");
            }

            foreach (var f in files)
                yield return f;

            var dirs = Array.Empty<string>();
            try
            {
                dirs = Directory.GetDirectories(current);
            }
            catch (Exception exe)
            {
                Trace.WriteLine($"Error accessing directories in directory {current}: {exe.Message}");
            }

            foreach (var d in dirs)
                stack.Push(d);
        }
    }

    /// <summary>
    /// Gets the full name of the type.
    /// </summary>
    /// <param name="typeDecl">The type decl.</param>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <returns>Full type name.</returns>
    internal static string GetTypeFullName(TypeDeclarationSyntax typeDecl, string? namespaceName = null)
    {
        var ns = namespaceName ?? (typeDecl.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
        return ns != null ? $"{ns}.{typeDecl.Identifier.Text}" : typeDecl.Identifier.Text;
    }

    /// <summary>
    /// Enumerates all relevant C# source files in a project directory.
    /// </summary>
    /// <param name="projectPath">The root project directory.</param>
    /// <returns>Enumerable of file paths.</returns>
    internal static IEnumerable<string> GetSourceFiles(string projectPath)
    {
        if (SourceFileCache.TryGetValue(projectPath, out var cached))
            return cached;

        var files = Directory
            .EnumerateFiles(projectPath, CoreResources.ResourceCsExtension, SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\bin\", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains(@"\.vs\", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("resource", StringComparison.OrdinalIgnoreCase) &&
                        !f.Contains("const", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        SourceFileCache[projectPath] = files;
        return files;
    }

    /// <summary>
    /// Determines whether a <c>for</c> loop has a constant numeric bound.
    /// </summary>
    /// <param name="loop">The <see cref="ForStatementSyntax"/> to analyze.</param>
    /// <returns>
    /// <see cref="LoopContext.ConstantBounded"/> if the loop's upper bound is a numeric literal;
    /// otherwise <see cref="LoopContext.VariableBounded"/>.
    /// </returns>
    private static LoopContext AnalyzeForLoop(ForStatementSyntax loop)
    {
        if (loop.Condition is BinaryExpressionSyntax { Right: LiteralExpressionSyntax literal } &&
            literal.IsKind(SyntaxKind.NumericLiteralExpression))
        {
            return LoopContext.ConstantBounded;
        }

        return LoopContext.VariableBounded;
    }

    /// <summary>
    /// Determines whether a given file should be ignored during analysis.
    /// </summary>
    /// <param name="filePath">The absolute path of the file.</param>
    /// <returns>
    /// <see langword="true"/> if the file is auto-generated or excluded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool ShouldIgnore(string filePath)
    {
        if (IgnoreCache.TryGetValue(filePath, out var cached))
            return cached;

        var fileName = Path.GetFileName(filePath);

        // Skip known generated or boilerplate files
        if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(@"\obj\", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(@"\bin\", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Skip files with <auto-generated> comments in the header
        try
        {
            if (File.ReadLines(filePath).Take(10).Any(line =>
                    line.Contains("<auto-generated", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }
        catch
        {
            // If file cannot be read, skip it to avoid exceptions
            return true;
        }

        return false;
    }
}
