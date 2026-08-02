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
using Microsoft.CodeAnalysis;
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
    /// Analyzer that uses Roslyn SemanticModel to detect non-escaping collection
    /// allocations inside loops that can be safely hoisted and cleared.
    /// </summary>
    public sealed class LoopAllocationAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "LoopAllocation";

        /// <inheritdoc cref="ICommand" />
        public string Description => "Detects non-escaping collection allocations inside loops.";

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

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            // 1. Build an in-memory compilation unit to get a SemanticModel
            var compilation = CSharpCompilation.Create("AnalysisAssembly")
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location))
                .AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            // Find explicit 'new Type()' and implicit 'new()' allocations
            var creations = root.DescendantNodes()
                .Where(node => node is ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax);

            foreach (var creationNode in creations)
            {
                var loopNode = creationNode.Ancestors().FirstOrDefault(IsLoopSyntax);
                if (loopNode == null)
                    continue;

                // 2. Resolve type symbol & verify it's a collection (and not string)
                var typeInfo = semanticModel.GetTypeInfo(creationNode);
                var typeSymbol = typeInfo.Type;
                if (typeSymbol == null || !IsCollectionType(typeSymbol))
                    continue;

                // 3. Extract the target local variable symbol receiving the allocation
                if (!TryGetLocalVariableSymbol(creationNode, semanticModel, out var localSymbol))
                {
                    // Assigned to a field, property, or unassigned -> skip (assume stateful/escaping)
                    continue;
                }

                // 4. Verify variable declaration is local to the loop
                if (!IsDeclaredInsideLoop(localSymbol, loopNode))
                {
                    // Declared outside loop -> reassignment/outer scope escape
                    continue;
                }

                // 5. Escape Analysis: Check if the reference escapes the loop body
                if (DoesSymbolEscape(localSymbol, loopNode, semanticModel))
                {
                    continue;
                }

                // Safe candidate for hoisting + .Clear()
                var line = creationNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    Enums.DiagnosticSeverity.Warning,
                    filePath,
                    line,
                    $"Collection '{typeSymbol.ToDisplayString()}' allocated inside loop does not escape. Hoist allocation outside loop and reuse via '.Clear()'.",
                    DiagnosticImpact.MemoryBound
                );
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
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
            sb.AppendLine("🗑️ Escape-Analyzed Loop Allocation Diagnostics:");
            sb.AppendLine(new string('-', 50));

            foreach (var d in results)
                sb.AppendLine(d.ToString());

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }

        /// <summary>
        /// Determines whether [is loop syntax] [the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if [is loop syntax] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsLoopSyntax(SyntaxNode node) =>
            node is ForStatementSyntax ||
            node is ForEachStatementSyntax ||
            node is WhileStatementSyntax ||
            node is DoStatementSyntax;

        /// <summary>
        /// Determines whether [is collection type] [the specified type symbol].
        /// </summary>
        /// <param name="typeSymbol">The type symbol.</param>
        /// <returns>
        ///   <c>true</c> if [is collection type] [the specified type symbol]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCollectionType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.SpecialType == SpecialType.System_String)
                return false;

            if (typeSymbol.TypeKind == TypeKind.Array)
                return true;

            return typeSymbol.AllInterfaces.Any(i =>
                i.SpecialType == SpecialType.System_Collections_IEnumerable ||
                i.ToDisplayString().StartsWith("System.Collections.Generic.ICollection"));
        }

        /// <summary>
        /// Tries the get local variable symbol.
        /// </summary>
        /// <param name="creationNode">The creation node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="localSymbol">The local symbol.</param>
        /// <returns></returns>
        private static bool TryGetLocalVariableSymbol(
            SyntaxNode creationNode,
            SemanticModel semanticModel,
            out ILocalSymbol? localSymbol)
        {
            localSymbol = null;

            // Pattern: var x = new List<T>();
            if (creationNode.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator })
            {
                localSymbol = semanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol;
                return localSymbol != null;
            }

            // Pattern: x = new List<T>();
            if (creationNode.Parent is AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier })
            {
                localSymbol = semanticModel.GetSymbolInfo(identifier).Symbol as ILocalSymbol;
                return localSymbol != null;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is declared inside loop] [the specified local symbol].
        /// </summary>
        /// <param name="localSymbol">The local symbol.</param>
        /// <param name="loopNode">The loop node.</param>
        /// <returns>
        ///   <c>true</c> if [is declared inside loop] [the specified local symbol]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsDeclaredInsideLoop(ILocalSymbol? localSymbol, SyntaxNode loopNode)
        {
            var declarationSyntax = localSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            return declarationSyntax != null && loopNode.Span.Contains(declarationSyntax.Span);
        }

        /// <summary>
        /// Doeses the symbol escape.
        /// </summary>
        /// <param name="localSymbol">The local symbol.</param>
        /// <param name="loopNode">The loop node.</param>
        /// <param name="semanticModel">The semantic model.</param>
        /// <returns>
        ///   <c>true</c> if [symbol escapes] [the specified loop]; otherwise, <c>false</c>.
        /// </returns>
        private static bool DoesSymbolEscape(ILocalSymbol? localSymbol, SyntaxNode loopNode,
            SemanticModel semanticModel)
        {
            // Find all identifier nodes in the loop pointing to this exact symbol
            var references = loopNode.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id =>
                    SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(id).Symbol, localSymbol));

            foreach (var reference in references)
            {
                // Escape 1: Returned or yielded from method
                if (reference.Ancestors().Any(a => a is ReturnStatementSyntax or YieldStatementSyntax))
                    return true;

                // Escape 2: Passed as an argument (e.g., outerList.Add(temp) or Process(temp))
                if (reference.Parent is ArgumentSyntax)
                    return true;

                // Escape 3: Assigned to external targets (e.g., this.Field = temp or outerVar = temp)
                if (reference.Parent is AssignmentExpressionSyntax assignment && assignment.Right == reference)
                    return true;

                // Escape 4: Captured by lambda or local function
                if (reference.Ancestors()
                    .Any(a => a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                    return true;
            }

            return false;
        }
    }
}