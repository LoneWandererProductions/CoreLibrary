/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DeadReferenceAnalyzer.cs
 * PURPOSE:     Checks code for unused project/assembly references.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Command that detects unused project/assembly references.
    /// </summary>
    /// <seealso cref="Weaver.Interfaces.ICommand" />
    public sealed class DeadReferenceAnalyzer : ICommand, ICodeAnalyzer
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Namespace => "analysis";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "deadrefs";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects unused project/assembly references.";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (!filePath.EndsWith(CoreResources.ResourceCsProjectExtension, StringComparison.OrdinalIgnoreCase))
                yield break;

            // FileContent is empty for .csproj execution → load file
            var xml = File.ReadAllText(filePath);
            var project = new ProjectReferenceInfo(xml);

            var unused = project.GetUnusedReferences();
            foreach (var r in unused)
            {
                yield return new Diagnostic(
                    Name,
                    Enums.DiagnosticSeverity.Warning,
                    filePath,
                    0,
                    $"Unused reference '{r}' detected.",
                    Enums.DiagnosticImpact.Other
                );
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: deadrefs <folderOrFile (.csproj)>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var output = string.Join("\n", results.Select(d => $"{Path.GetFileName(d.FilePath)} -> {d.Message}"));
            return CommandResult.Ok($"Unused references detected:\n{output}", results);
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}