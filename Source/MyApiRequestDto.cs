#pragma warning disable CA1822

using Metagen.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen.Source;

public record MyApiRequestDto(string Id)
{
    public int SumTwoNumbers()
    {
        return 0;
    }

    public Dictionary<string, object?> ToDictionary()
        => new()
        {
        };

    public void Foo(int bar)
    {
    }

    private interface IRewriteMyApiRequestDto : IRewriteStuff
    {
        static TypeDeclarationSyntax MyApiRequestDto(
            TypeDeclarationSyntax typeDeclarationSyntax,
            string name)
            => typeDeclarationSyntax
                .WithIdentifier(SyntaxFactory.Identifier($"{name}Request"));

        static TypeDeclarationSyntax MyApiRequestDto(
            TypeDeclarationSyntax typeDeclarationSyntax,
            Type type,
            string name)
            => typeDeclarationSyntax
                .AddParameter(type.Name, name);
        
        static MethodDeclarationSyntax ToDictionary(
            MethodDeclarationSyntax methodDeclarationSyntax,
            Type _, 
            string name)
        {
            var initializerExpressionSyntax = methodDeclarationSyntax
                .DescendantNode<InitializerExpressionSyntax>();

            return methodDeclarationSyntax
                .ReplaceNode(
                    initializerExpressionSyntax,
                    initializerExpressionSyntax
                        .AddInitializer(
                            key: SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(name)
                            ),
                            value: SyntaxFactory.IdentifierName(name)
                        )
                );
        }

        // Random stuff
        static ParameterSyntax Bar(ParameterSyntax parameterSyntax, int value)
        {
            return parameterSyntax.WithIdentifier(SyntaxFactory.Identifier("lol"));
        }

        static MethodDeclarationSyntax SumTwoNumbers(
            MethodDeclarationSyntax methodDeclarationSyntax,
            int value)
            => methodDeclarationSyntax
                .ReturnExpression<int>(
                    (left, right) => left + right,
                    value);
    }
}
