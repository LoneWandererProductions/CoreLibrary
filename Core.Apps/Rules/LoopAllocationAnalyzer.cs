/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        LoopAllocationAnalyzer.cs
 * PURPOSE:     Analyzer that detects potential memory spikes, create anew enumerable in a loop,
 *              instead of declaring it local and clearing it.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using Core.Apps.Enums;
using Core.Apps.Helper;
using Core.Apps.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects collection instantiations inside loops to prevent GC pressure.
    /// </summary>
    public sealed class LoopAllocationAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "LoopAllocation";

        /// <inheritdoc cref="ICommand" />
        public string Description => "Detects collection allocations inside loops that could be reused or cleared.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// The collection type names
        /// </summary>
        private static readonly HashSet<string> CollectionTypeNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "List",
            "Dictionary",
            "HashSet",
            "Queue",
            "Stack",
            "Collection",
            "LinkedList",
            "ConcurrentBag",
            "ConcurrentDictionary"
        };

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            // 1. Explicit 'new Type()' creations
            foreach (var creation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                var typeName = creation.Type.ToString();
                if (!IsCollectionType(typeName))
                    continue;

                if (CoreHelper.GetLoopContext(creation) == LoopContext.None)
                    continue;

                var line = creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Warning,
                    filePath,
                    line,
                    $"Collection '{typeName}' allocated inside a loop with 'new'. Consider moving allocation outside the loop and calling '.Clear()' to reduce GC pressure.",
                    DiagnosticImpact.MemoryBound
                );
            }

            // 2. Implicit 'new()' creations
            foreach (var implicitCreation in root.DescendantNodes().OfType<ImplicitObjectCreationExpressionSyntax>())
            {
                if (CoreHelper.GetLoopContext(implicitCreation) == LoopContext.None)
                    continue;

                // Simple check for variable target type name if available
                var variableDeclaration = implicitCreation.Ancestors().OfType<VariableDeclarationSyntax>().FirstOrDefault();
                var typeName = variableDeclaration?.Type.ToString() ?? "implicit collection";

                if (variableDeclaration != null && !IsCollectionType(typeName))
                    continue;

                var line = implicitCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Warning,
                    filePath,
                    line,
                    $"Collection creation 'new()' detected inside a loop. Consider hoisting allocation outside the loop and calling '.Clear()'.",
                    DiagnosticImpact.MemoryBound
                );
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: LoopAllocation <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var sb = new StringBuilder();
            sb.AppendLine("🗑️ Loop Allocation Diagnostics:");
            sb.AppendLine(new string('-', 50));

            foreach (var d in results)
                sb.AppendLine(d.ToString());

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }

        /// <summary>
        /// Determines whether [is collection type] [the specified type name].
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>
        ///   <c>true</c> if [is collection type] [the specified type name]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCollectionType(string typeName)
        {
            return CollectionTypeNames.Any(c => typeName.StartsWith(c, StringComparison.OrdinalIgnoreCase))
                   || typeName.EndsWith("[]", StringComparison.Ordinal);
        }
    }
}
