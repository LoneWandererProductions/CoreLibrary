/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        DisposableOwnershipAnalyzer.cs
 * PURPOSE:     Semantic-model based analyzer that covers two gaps CA2000 (and our own
 *              DisposableAnalyzer) leave open:
 *
 *              1. CA2000 only tracks LOCAL variables. It intentionally does not flag an
 *                 instance field or auto-property being overwritten with a new
 *                 IDisposable value while the previous value was never disposed, because
 *                 a field's lifetime is tied to its containing object and CA2000 can't
 *                 reason about that object's overall disposal contract from one method.
 *                 In practice this means "_bitmap = newBitmap;" silently drops the old
 *                 GDI handle and CA2000 says nothing. This analyzer flags it.
 *
 *              2. CA2000 (and DisposableAnalyzer) give every undisposed-disposable
 *                 finding the same severity, whether it happens once on a button click
 *                 or once per file in a 10,000-file batch scan. This analyzer reuses the
 *                 same loop classification CoreHelper/HotPathAnalyzer already use to
 *                 escalate severity and impact when the leak sits inside a loop, so
 *                 "leaks per iteration" is visibly worse than "leaks once".
 *
 *              It also replaces DisposableAnalyzer's name-suffix heuristic ("Stream" /
 *              "Reader" / "Writer") with real semantic type resolution wherever the
 *              compilation can resolve the type, falling back to a heuristic (with a
 *              wider, domain-relevant name list) only when it can't. AnalyzeProject is
 *              implemented so that project-local disposable types declared in a
 *              different file than where they're used (e.g. a custom DirectBitmap
 *              wrapper) still resolve correctly - something a single-file compilation
 *              can never see.
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
using DiagnosticSeverity = Core.Apps.Enums.DiagnosticSeverity;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects disposable ownership bugs CA2000 does not cover: field/local
    /// overwrite-before-dispose, with severity escalated when the bug sits inside a loop.
    /// </summary>
    public sealed class DisposableOwnershipAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "DisposableOwnership";

        /// <inheritdoc cref="ICommand" />
        public string Description =>
            "Detects disposable fields/locals overwritten before disposal, and undisposed disposables, " +
            "escalating severity when the bug runs inside a loop.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Widened fallback name list, used only when the semantic model can't resolve a
        /// type (e.g. an external assembly not referenced by the lightweight in-memory
        /// compilation). Beyond DisposableAnalyzer's I/O-centric list this adds the GDI+ /
        /// imaging vocabulary that a name-only heuristic previously missed entirely.
        /// </summary>
        private static readonly string[] HeuristicDisposableHints =
        {
            "Stream", "Reader", "Writer", "Bitmap", "Image", "Graphics", "Font", "Pen", "Brush", "Handle", "Timer",
            "Connection", "Command", "Context", "Socket"
        };

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
            {
                yield break;
            }

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var compilation = BuildCompilation(new[] { tree });
            var semanticModel = compilation.GetSemanticModel(tree);

            foreach (var diagnostic in AnalyzeTree(filePath, tree.GetRoot(), semanticModel))
            {
                yield return diagnostic;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Project-wide pass: every file is parsed into one shared compilation before any
        /// file is analyzed, so a disposable type declared in FileA.cs (e.g. an internal
        /// wrapper like DirectBitmap) still resolves correctly for a field/local declared
        /// in FileB.cs. A per-file Analyze() call can never do this on its own.
        /// </remarks>
        public IEnumerable<Diagnostic> AnalyzeProject(Dictionary<string, string> allFiles)
        {
            var trees = new List<(string? Path, SyntaxTree Tree)>();

            foreach (var (path, content) in allFiles)
            {
                if (CoreHelper.ShouldIgnoreFile(path))
                {
                    continue;
                }

                trees.Add((path, CSharpSyntaxTree.ParseText(content, path: path ?? string.Empty)));
            }

            var compilation = BuildCompilation(trees.Select(t => t.Tree));

            foreach (var (path, tree) in trees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                foreach (var diagnostic in AnalyzeTree(path, tree.GetRoot(), semanticModel))
                {
                    yield return diagnostic;
                }
            }
        }

        /// <summary>
        /// Builds the shared in-memory compilation used for semantic type resolution.
        /// </summary>
        private static CSharpCompilation BuildCompilation(IEnumerable<SyntaxTree> trees)
        {
            return CSharpCompilation.Create("DisposableOwnershipAnalysis")
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location))
                .AddSyntaxTrees(trees);
        }

        /// <summary>
        /// Runs both checks (local overwrite/never-disposed, field overwrite) over one file.
        /// </summary>
        private static IEnumerable<Diagnostic> AnalyzeTree(string? filePath, SyntaxNode root,
            SemanticModel semanticModel)
        {
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                foreach (var diagnostic in AnalyzeLocalsInMethod(filePath, method, semanticModel))
                {
                    yield return diagnostic;
                }

                // Field overwrite is only meaningful outside constructors: the first
                // assignment to a field has nothing previous to leak.
                foreach (var diagnostic in AnalyzeFieldOverwritesInMethod(filePath, method, semanticModel))
                {
                    yield return diagnostic;
                }
            }
        }

        // ----------------------------------------------------------------------------
        // Local variables: reassignment-before-dispose, and never-disposed (semantic).
        // ----------------------------------------------------------------------------

        private static IEnumerable<Diagnostic> AnalyzeLocalsInMethod(string? filePath,
            MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            if (method.Body == null)
            {
                yield break;
            }

            foreach (var declarator in method.Body.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                if (declarator.Initializer?.Value == null)
                {
                    continue;
                }

                if (!IsDisposableExpression(declarator.Initializer.Value, semanticModel, out var typeName))
                {
                    continue;
                }

                if (IsWrappedInUsing(declarator))
                {
                    continue;
                }

                var localSymbol = semanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol;

                // 1. Overwrite check: identifier reassigned before the value it held is
                //    disposed. This is the case a plain "is Dispose() called anywhere in
                //    the method" scan (the old heuristic) cannot tell apart from a real fix,
                //    because it only checks presence, not order relative to the overwrite.
                var reassignments = method.Body.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(a => a.Left is IdentifierNameSyntax id &&
                                id.Identifier.Text == declarator.Identifier.Text &&
                                a.SpanStart > declarator.SpanStart)
                    .Where(a => localSymbol == null ||
                                SymbolEqualityComparer.Default.Equals(
                                    semanticModel.GetSymbolInfo((IdentifierNameSyntax)a.Left).Symbol, localSymbol));

                foreach (var reassignment in reassignments)
                {
                    if (HasDisposeCallBefore(method, declarator.Identifier.Text, reassignment.SpanStart))
                    {
                        continue;
                    }

                    yield return BuildDiagnostic(filePath, reassignment,
                        $"'{declarator.Identifier.Text}' ({typeName}) is reassigned here, but the disposable " +
                        "value it held before this line was never disposed. Each reassignment silently drops " +
                        "the previous instance.");
                }

                // 2. Never-disposed-at-all check, but only when we could actually prove the
                //    type is disposable (semantic match, or heuristic fallback) - unlike
                //    DisposableAnalyzer this does not depend on the variable's own name.
                if (!HasDisposeCallBefore(method, declarator.Identifier.Text, int.MaxValue) &&
                    !DoesEscape(declarator.Identifier.Text, method, semanticModel))
                {
                    yield return BuildDiagnostic(filePath, declarator,
                        $"'{declarator.Identifier.Text}' ({typeName}) is never disposed and does not escape " +
                        $"'{method.Identifier.Text}'. Use 'using' or call .Dispose().");
                }
            }
        }

        /// <summary>
        /// True if the identifier is returned, passed as an argument, or assigned to a
        /// field/property - i.e. ownership plausibly transfers out of this method.
        /// </summary>
        private static bool DoesEscape(string identifierName, MethodDeclarationSyntax method,
            SemanticModel semanticModel)
        {
            var references = method.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => id.Identifier.Text == identifierName);

            foreach (var reference in references)
            {
                if (reference.Ancestors().Any(a => a is ReturnStatementSyntax or YieldStatementSyntax))
                {
                    return true;
                }

                if (reference.Parent is ArgumentSyntax)
                {
                    return true;
                }

                if (reference.Parent is AssignmentExpressionSyntax { Left: not IdentifierNameSyntax } assignment &&
                    assignment.Right == reference)
                {
                    // Assigned to something other than plain re-binding of itself, e.g. a
                    // field ("_field = local;") or member access ("obj.Prop = local;").
                    return true;
                }
            }

            return false;
        }

        // ----------------------------------------------------------------------------
        // Fields: the check CA2000 explicitly does not perform.
        // ----------------------------------------------------------------------------

        private static IEnumerable<Diagnostic> AnalyzeFieldOverwritesInMethod(string? filePath,
            MethodDeclarationSyntax method, SemanticModel semanticModel)
        {
            if (method.Body == null || method.Identifier.Text.StartsWith("Dispose", StringComparison.Ordinal))
            {
                yield break;
            }

            var assignments = method.Body.DescendantNodes().OfType<AssignmentExpressionSyntax>();

            foreach (var assignment in assignments)
            {
                var fieldName = GetAssignedFieldName(assignment, semanticModel, out var fieldTypeName);
                if (fieldName == null)
                {
                    continue;
                }

                if (HasDisposeCallBefore(method, fieldName, assignment.SpanStart, allowThisPrefix: true))
                {
                    continue;
                }

                yield return BuildDiagnostic(filePath, assignment,
                    $"Field '{fieldName}' ({fieldTypeName}) is reassigned in '{method.Identifier.Text}' without " +
                    "disposing the value it held before this line. CA2000 does not check this - it only tracks " +
                    "local variables, not fields - so this class of leak passes standard analysis silently.");
            }
        }

        /// <summary>
        /// If the assignment's left side is a disposable-typed instance field (optionally
        /// through 'this.'), returns its name and resolved (or heuristic) type name.
        /// </summary>
        private static string? GetAssignedFieldName(AssignmentExpressionSyntax assignment,
            SemanticModel semanticModel, out string fieldTypeName)
        {
            fieldTypeName = string.Empty;

            ExpressionSyntax target = assignment.Left switch
            {
                MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } member => member.Name,
                IdentifierNameSyntax identifier => identifier,
                _ => null!
            };

            if (target == null)
            {
                return null;
            }

            var symbol = semanticModel.GetSymbolInfo(target).Symbol;
            if (symbol is not IFieldSymbol fieldSymbol || fieldSymbol.IsReadOnly)
            {
                return null;
            }

            if (!IsDisposableType(fieldSymbol.Type))
            {
                return null;
            }

            fieldTypeName = fieldSymbol.Type.ToDisplayString();
            return fieldSymbol.Name;
        }

        // ----------------------------------------------------------------------------
        // Shared helpers
        // ----------------------------------------------------------------------------

        /// <summary>
        /// Resolves whether an initializer expression produces a disposable value, using
        /// the semantic model first and a widened name heuristic only when the type can't
        /// be resolved (e.g. external assembly not present in this lightweight compilation).
        /// </summary>
        private static bool IsDisposableExpression(ExpressionSyntax expression, SemanticModel semanticModel,
            out string typeName)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression);
            if (typeInfo.Type != null && typeInfo.Type.TypeKind != TypeKind.Error)
            {
                typeName = typeInfo.Type.ToDisplayString();
                return IsDisposableType(typeInfo.Type);
            }

            // Fallback: unresolved type (common for external/imaging assemblies in a
            // single-file compilation) - use the widened name heuristic instead of giving up.
            var candidateName = expression switch
            {
                ObjectCreationExpressionSyntax creation => creation.Type.ToString(),
                InvocationExpressionSyntax invocation => invocation.Expression.ToString(),
                _ => expression.ToString()
            };

            typeName = candidateName;
            return HeuristicDisposableHints.Any(hint =>
                candidateName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsDisposableType(ITypeSymbol? type)
        {
            if (type == null || type.TypeKind == TypeKind.Error)
            {
                return false;
            }

            return type.ToDisplayString() == "System.IDisposable" ||
                   type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable");
        }

        private static bool IsWrappedInUsing(VariableDeclaratorSyntax variable)
        {
            if (variable.Parent?.Parent is UsingStatementSyntax)
            {
                return true;
            }

            return variable.Parent?.Parent is LocalDeclarationStatementSyntax localDecl &&
                   localDecl.UsingKeyword.IsKind(SyntaxKind.UsingKeyword);
        }

        /// <summary>
        /// Approximates flow order using source position: true if a "&lt;name&gt;.Dispose()"
        /// or "&lt;name&gt;?.Dispose()" (or with "this." prefix when allowThisPrefix) call appears
        /// earlier in the method than beforePosition.
        /// </summary>
        private static bool HasDisposeCallBefore(MethodDeclarationSyntax method, string name, int beforePosition,
            bool allowThisPrefix = false)
        {
            // 1. Direct calls: _current.Dispose() or this._current.Dispose()
            var standardCalls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.SpanStart < beforePosition)
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax member &&
                            member.Name.Identifier.Text == "Dispose" &&
                            IsTargetVariable(member.Expression, name, allowThisPrefix));

            if (standardCalls)
            {
                return true;
            }

            // 2. Null-conditional calls: _current?.Dispose() or this._current?.Dispose()
            var conditionalCalls = method.DescendantNodes()
                .OfType<ConditionalAccessExpressionSyntax>()
                .Where(cond => cond.SpanStart < beforePosition)
                .Any(cond => IsTargetVariable(cond.Expression, name, allowThisPrefix) &&
                             IsDisposeInvocation(cond.WhenNotNull));

            return conditionalCalls;
        }

        /// <summary>
        /// Helper to check if an expression matches the variable name (with optional 'this.' prefix).
        /// </summary>
        private static bool IsTargetVariable(ExpressionSyntax expression, string name, bool allowThisPrefix)
        {
            if (expression is IdentifierNameSyntax id && id.Identifier.Text == name)
            {
                return true;
            }

            if (allowThisPrefix && expression is MemberAccessExpressionSyntax
                {
                    Expression: ThisExpressionSyntax, Name.Identifier.Text: var inner
                } &&
                inner == name)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper to check if a conditionally accessed expression executes Dispose().
        /// </summary>
        private static bool IsDisposeInvocation(ExpressionSyntax whenNotNull)
        {
            if (whenNotNull is InvocationExpressionSyntax inv)
            {
                if (inv.Expression is MemberBindingExpressionSyntax binding &&
                    binding.Name.Identifier.Text == "Dispose")
                {
                    return true;
                }

                if (inv.Expression is MemberAccessExpressionSyntax member &&
                    member.Name.Identifier.Text == "Dispose")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Builds the diagnostic for a given node, escalating severity/impact when the node
        /// sits inside a loop - the correlation CA2000 has no mechanism to express.
        /// </summary>
        private static Diagnostic BuildDiagnostic(string? filePath, SyntaxNode node, string message)
        {
            var loopContext = CoreHelper.GetLoopContext(node);
            var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            var severity = loopContext == LoopContext.None
                ? DiagnosticSeverity.Warning
                : DiagnosticSeverity.Error;

            var suffix = loopContext switch
            {
                LoopContext.Nested =>
                    " Runs inside nested loops - every leaked instance multiplies per outer iteration.",
                LoopContext.VariableBounded =>
                    " Runs inside a loop with a variable bound (e.g. a file/collection scan) - the leak scales with input size, not code size.",
                LoopContext.ConstantBounded =>
                    " Runs inside a fixed-size loop - repeats a known number of times per call.",
                _ => string.Empty
            };

            return new Diagnostic(
                "DisposableOwnership",
                severity,
                filePath,
                line,
                message + suffix,
                DiagnosticImpact.MemoryBound);
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: DisposableOwnership <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var sb = new StringBuilder();
            sb.AppendLine("♻️️ Disposable Ownership Diagnostics:");
            sb.AppendLine(new string('-', 50));

            foreach (var d in results)
            {
                sb.AppendLine(d.ToString());
            }

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }
    }
}
