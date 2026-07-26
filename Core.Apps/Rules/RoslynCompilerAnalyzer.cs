/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        RoslynCompilerAnalyzer.cs
 * PURPOSE:     Analyzer that runs the C# compiler to collect native warnings and errors.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Helper;
using Core.Apps.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = Core.Apps.Enums.DiagnosticSeverity;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that invokes the Roslyn compiler on a file or directory 
    /// and captures all compiler errors and warnings in the application's native format.
    /// </summary>
    public sealed class RoslynCompilerAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "RoslynCompiler";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Collects native Roslyn compiler warnings and errors.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            // 1. Build a syntax tree for the single file
            var tree = CSharpSyntaxTree.ParseText(fileContent, path: filePath ?? string.Empty);

            // 2. Dynamically gather all loaded assemblies in the AppDomain as metadata references.
            // This ensures full visibility into LINQ, WPF, runtime libraries, and cross-project types.
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location) && File.Exists(a.Location))
                .Select(a => a.Location)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(loc => MetadataReference.CreateFromFile(loc))
                .Cast<MetadataReference>()
                .ToList();

            // Fallback safety if AppDomain references list is empty
            if (references.Count == 0)
            {
                references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            }

            // 3. Setup compilation with comprehensive references
            var compilation = CSharpCompilation.Create(
                assemblyName: Path.GetFileNameWithoutExtension(filePath) ?? "DynamicAssembly",
                syntaxTrees: new[] { tree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            // 4. Get all diagnostics (errors, warnings, info) from the compilation step
            var roslynDiagnostics = compilation.GetDiagnostics();

            foreach (var diag in roslynDiagnostics)
            {
                // Filter out hidden diagnostics or ones not relevant to this specific file tree
                if (diag.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden)
                    continue;

                // Map Roslyn severity to your application's DiagnosticSeverity
                var severity = diag.Severity switch
                {
                    Microsoft.CodeAnalysis.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
                    Microsoft.CodeAnalysis.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                    _ => DiagnosticSeverity.Info
                };

                // Extract line number (Roslyn line numbers are 0-indexed, your Diagnostic class expects 1-indexed)
                var lineSpan = diag.Location.GetLineSpan();
                int lineNumber = lineSpan.StartLinePosition.Line + 1;
                string targetFile = lineSpan.Path ?? filePath ?? string.Empty;

                // Format message to include the Roslyn error code (e.g., CS0168) for context
                string message = $"{diag.Id}: {diag.GetMessage()}";
                const string source = "roslyn";

                yield return new Diagnostic(
                    Name,
                    severity,
                    targetFile,
                    lineNumber,
                    message,
                    null,
                    source
                );
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            List<Diagnostic> diagnostics;

            try
            {
                diagnostics = AnalyzerExecutor.ExecutePath(
                    this,
                    args,
                    "Usage: RoslynCompiler <fileOrDirectoryPath>"
                );
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"RoslynCompiler execution failed: {ex.Message}");
            }

            if (diagnostics.Count == 0)
                return CommandResult.Ok($"No compilation issues found in '{args[0]}'.");

            var sb = new StringBuilder();
            sb.AppendLine($"Roslyn Compilation Report for: {args[0]}");
            sb.AppendLine(new string('-', 80));

            foreach (var d in diagnostics)
                sb.AppendLine(d.ToString());

            sb.AppendLine(new string('-', 80));
            sb.AppendLine($"{diagnostics.Count} compilation issue(s) detected.");

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }
    }
}
