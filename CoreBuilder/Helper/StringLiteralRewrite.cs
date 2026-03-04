/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        StringLiteralRewrite.cs
 * PURPOSE:     Replace string literals in C# code with resource references.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Helper
{
    /// <inheritdoc />
    /// <summary>
    ///     Converts string literals in C# code to resource references.
    /// </summary>
    /// <seealso cref="T:Microsoft.CodeAnalysis.CSharp.CSharpSyntaxRewriter" />
    internal sealed class StringLiteralRewrite : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     The string to resource map
        /// </summary>
        private readonly Dictionary<string, string> _stringToResourceMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringLiteralRewrite" /> class.
        /// </summary>
        /// <param name="stringToResourceMap">The string to resource map.</param>
        internal StringLiteralRewrite(Dictionary<string, string> stringToResourceMap)
        {
            _stringToResourceMap = stringToResourceMap;
        }

        /// <summary>
        ///     Rewrites the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Rewritten Code</returns>
        internal string Rewrite(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var newRoot = Visit(root);
            return newRoot.NormalizeWhitespace().ToFullString();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Visits the literal expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Replace string with expression.</returns>
        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax? node)
        {
            if (node == null)
            {
                return null;
            }

            if (!node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return base.VisitLiteralExpression(node);
            }

            var value = node.Token.ValueText;

            if (!_stringToResourceMap.TryGetValue(value, out var resourceName))
            {
                return base.VisitLiteralExpression(node);
            }

            return SyntaxFactory.ParseExpression($"Resource.{resourceName}")
                .WithTriviaFrom(node);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Visits the interpolated string expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Interpolated string as replacement for existing string.</returns>
        public override SyntaxNode? VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            var staticParts = new List<string>();
            var placeholderArgs = new List<string>();
            var index = 0;

            foreach (var content in node.Contents)
            {
                switch (content)
                {
                    case InterpolatedStringTextSyntax text:
                        staticParts.Add(text.TextToken.ValueText);
                        break;

                    case InterpolationSyntax interpolation:
                        staticParts.Add($"{{{index}}}");
                        placeholderArgs.Add(interpolation.Expression.ToString());
                        index++;
                        break;
                }
            }

            var extracted = string.Concat(staticParts);

            if (!_stringToResourceMap.TryGetValue(extracted, out var resourceName))
            {
                return base.VisitInterpolatedStringExpression(node);
            }

            string formatCall;

            if (placeholderArgs.Count == 0)
            {
                // No placeholders: use direct string reference
                formatCall = $"Resource.{resourceName}";
            }
            else
            {
                var args = string.Join(", ", placeholderArgs);
                formatCall = $"string.Format(Resource.{resourceName}, {args})";
            }

            return SyntaxFactory.ParseExpression(formatCall)
                .WithTriviaFrom(node);
        }
    }
}