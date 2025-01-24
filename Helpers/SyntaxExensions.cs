using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mutagen.Helpers;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metagen.Helpers;

internal static class SyntaxExensions
{
    public static TypeDeclarationSyntax WithIdentifier(
        this TypeDeclarationSyntax syntaxNode,
        string text)
        => syntaxNode.WithIdentifier(Identifier(text));

    public static IEnumerable<T> DescendantNodes<T>(
        this SyntaxNode syntaxNode)
        where T : SyntaxNode
        => syntaxNode
            .DescendantNodes()
            .OfType<T>();

    public static T DescendantNode<T>(
        this SyntaxNode syntaxNode)
        where T : SyntaxNode
        => syntaxNode
            .DescendantNodes<T>()
            .Single();

    public static T DescendantNode<T>(
        this SyntaxNode syntaxNode,
        Func<T, bool> predicate)
        where T : SyntaxNode
        => syntaxNode
            .DescendantNodes<T>()
            .Single(predicate);

    public static TypeDeclarationSyntax AddParameter(
        this TypeDeclarationSyntax typeDeclarationSyntax,
        string type,
        string name)
            => typeDeclarationSyntax
                .AddParameterListParameters(
                    Parameter(
                        Identifier(name))
                            .WithType(
                                ParseTypeName(type)
                                    .WithTrailingTrivia(Whitespace(" "))));

    public static MethodDeclarationSyntax ReturnExpression<T>(
        this MethodDeclarationSyntax methodDeclarationSyntax,
        Expression<Func<T, T, T>> expression,
        int right)
        => methodDeclarationSyntax.ReturnExpression(
            expression,
            LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(right)));

    public static MethodDeclarationSyntax ReturnExpression<T>(
        this MethodDeclarationSyntax methodDeclarationSyntax,
        Expression<Func<T, T, T>> expression,
        LiteralExpressionSyntax right)
    {
        var body = methodDeclarationSyntax
            .Body
            ?? throw new InvalidOperationException();

        var returnStatement = body
            .Statements
            .OfType<ReturnStatementSyntax>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException();

        var returnStatementExpression = returnStatement.Expression
            ?? throw new InvalidOperationException();

        var returnValueExpression = expression
            .ToExpressionSyntax(
                replacements: new()
                {
                    ["left"] = returnStatementExpression.ToString(),
                    ["right"] = right.ToString()
                }
            );

        return methodDeclarationSyntax
            .WithBody(
                body
                .ReplaceNode(
                    returnStatement,
                    returnStatement
                        .WithExpression(returnValueExpression)));
    }

    public static InitializerExpressionSyntax AddInitializer(
        this InitializerExpressionSyntax initializerExpressionSyntax,
        string key,
        string value)
        => initializerExpressionSyntax
            .AddInitializer(
                key: LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(key)
                ),
                value: LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(value)
                ));

    public static InitializerExpressionSyntax AddInitializer(
        this InitializerExpressionSyntax initializerExpressionSyntax,
        ExpressionSyntax key,
        ExpressionSyntax value)
        => initializerExpressionSyntax
            .AddExpressions(
                DictionaryInitializer(key, value)
            );

    public static InitializerExpressionSyntax AddInitializer<TInput, TOutput>(
        this InitializerExpressionSyntax initializerExpressionSyntax,
        ExpressionSyntax key,
        Expression<Func<TInput, TOutput>> value,
        Dictionary<string, string> replacements)
            => initializerExpressionSyntax
                .AddExpressions(
                    DictionaryInitializer(
                        key,
                        value.ToExpressionSyntax(replacements))
                );

    private static AssignmentExpressionSyntax DictionaryInitializer(
        ExpressionSyntax key,
        ExpressionSyntax value)
        => AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            ImplicitElementAccess(
                BracketedArgumentList([Argument(key)])
            ),
            value
        );
}

