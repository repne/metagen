using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mutagen.Helpers;

public static class ExpressionExtensions
{
    public static ExpressionSyntax ToExpressionSyntax(
        this Expression expression)
        => CSharpSyntaxTree.ParseText(expression.ToString())
            .GetRoot()
            .DescendantNodes()
            .OfType<ExpressionStatementSyntax>()
            .Single()
            .Expression;

    public static ExpressionSyntax ToExpressionSyntax(
        this LambdaExpression expression,
        Dictionary<string, string> replacements)
        => new IdentifierNameRewriter(replacements)
            .Visit(expression.Body.ToExpressionSyntax())
            as ExpressionSyntax
            ?? throw new InvalidOperationException();

    private class IdentifierNameRewriter(
        Dictionary<string, string> identifierMap)
        : CSharpSyntaxRewriter
    {
        public override IdentifierNameSyntax VisitIdentifierName(IdentifierNameSyntax node)
            => identifierMap.TryGetValue(node.Identifier.ValueText, out var newIdentifier)
                ? SyntaxFactory.IdentifierName(newIdentifier)
                : node;
    }
}