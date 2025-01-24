using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen.Source;

public class MyApiClient(HttpClient httpClient)
{
    public async Task<HttpResponseMessage> SaveAsync(MyApiRequestDto request)
        => await httpClient.PutAsync(
            request.Endpoint,
            request.ToFormUrlEncodedContent());

    private interface IRewriteMyApiClient : IRewriteStuff
    {
        static MethodDeclarationSyntax SaveAsync(
            MethodDeclarationSyntax methodDeclarationSyntax,
            string name)
        {
            var identifiers = methodDeclarationSyntax
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(x => x.Identifier.ValueText == nameof(MyApiRequestDto))
                .ToArray();

            return methodDeclarationSyntax
                .ReplaceNodes(
                    identifiers,
                    (x, y) => SyntaxFactory.IdentifierName($"{name}Request").WithTriviaFrom(x)
                )
                .WithIdentifier(SyntaxFactory.Identifier($"Save{name}Async"));
        }
    }
}
