using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder
{
    internal sealed class StringLiteralRewrite : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _stringToResourceMap;

        internal StringLiteralRewrite(Dictionary<string, string> stringToResourceMap)
        {
            _stringToResourceMap = stringToResourceMap;
        }

        internal string Rewrite(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            var newRoot = Visit(root);
            return newRoot.NormalizeWhitespace().ToFullString();
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
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

        public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
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
