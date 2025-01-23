#pragma warning disable CA1822

using Metagen.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen.Source;

public record RequestInput()
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

    private interface IRewriteRequestInput : IRewriteStuff
    {
        static ParameterSyntax Bar(ParameterSyntax parameterSyntax, int value)
        {
            return parameterSyntax.WithIdentifier(SyntaxFactory.Identifier("lol"));
        }
        
        static TypeDeclarationSyntax RequestInput(
            TypeDeclarationSyntax typeDeclarationSyntax,
            string prefix)
            => typeDeclarationSyntax
                .WithIdentifier(SyntaxFactory.Identifier($"{prefix}{nameof(RequestInput)}"));

        static TypeDeclarationSyntax RequestInput(
            TypeDeclarationSyntax typeDeclarationSyntax,
            Type type,
            string name)
            => typeDeclarationSyntax
                .AddParameter(type.Name, name);

        static MethodDeclarationSyntax SumTwoNumbers(
            MethodDeclarationSyntax methodDeclarationSyntax,
            int value)
            => methodDeclarationSyntax
                .ReturnExpression<int>(
                    (left, right) => left + right,
                    value);
        
        static MethodDeclarationSyntax ToDictionary(
            MethodDeclarationSyntax methodDeclarationSyntax,
            string key,
            string value)
        {
            var initializerExpressionSyntax = methodDeclarationSyntax
                .DescendantNodes()
                .OfType<InitializerExpressionSyntax>()
                .First();

            return methodDeclarationSyntax
                .ReplaceNode(
                    initializerExpressionSyntax,
                    initializerExpressionSyntax
                        .AddInitializer(key, value)
                )
                .WithTriviaFrom(methodDeclarationSyntax);
        }
    }
}
