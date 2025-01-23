using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metagen.Helpers;

internal static class SyntaxExensions
{
    public static IEnumerable<T> DescendantNodes<T>(
        this SyntaxNode syntaxNode)
        where T: SyntaxNode
        => syntaxNode
            .DescendantNodes()
            .OfType<T>();

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

        var returnValueExpression = CreateBinaryExpression(
            expression,
            left: returnStatementExpression,
            right: right);

        return methodDeclarationSyntax
            .WithBody(
                body
                .ReplaceNode(
                    returnStatement,
                    returnStatement
                        .WithExpression(returnValueExpression)));
    }

    private static ExpressionSyntax CreateBinaryExpression<T>(
        Expression<Func<T, T, T>> expression,
        ExpressionSyntax left,
        ExpressionSyntax right)
    {
        var body = (expression as LambdaExpression).Body.ToString();
        var tree = CSharpSyntaxTree.ParseText(body);
        var node = tree
            .GetRoot()
            .DescendantNodes()
            .OfType<ExpressionStatementSyntax>()
            .Single()
            .Expression;

        var leftNode = node
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(x => x.Identifier.ValueText == "left")
            .Single();

        node =  node
            .ReplaceNode(leftNode, left);
        
        var rightNode = node
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(x => x.Identifier.ValueText == "right")
            .Single();
        
        node =  node
            .ReplaceNode(rightNode, right);
        
        return node;
    }

    public static InitializerExpressionSyntax AddInitializer(
        this InitializerExpressionSyntax initializerExpressionSyntax,
        string key,
        string value)
        => initializerExpressionSyntax
            .AddExpressions(DictionaryInitializer(key, value));

    private static AssignmentExpressionSyntax DictionaryInitializer(string key, string value)
        => AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            ImplicitElementAccess(
                BracketedArgumentList(
                    [
                        Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(key)
                            )
                        )
                    ]
                )
            ),
            LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(value)
            )
        );
}

