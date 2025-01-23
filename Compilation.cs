using Metagen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

internal sealed record Compilation
{
    private Stack<TypeBuilder> TypeBuilders { get; init; } = [];
    private List<TypeBuilder> CompletedTypeBuilders { get; init; } = [];

    public void Start(Type subjectType)
        => TypeBuilders.Push(TypeBuilder.Create(subjectType));

    public void Complete()
        => CompletedTypeBuilders.Add(TypeBuilders.Pop());

    public long Pending
        => TypeBuilders.Count;

    public Compilation Broadcast(params object?[] args)
        => this with
        {
            TypeBuilders = new(
                TypeBuilders.Select(typeBuilder => typeBuilder.Build(args))
            )
        };
    
    public IEnumerable<string> ToFullStrings()
    {
        using var workspace = new AdhocWorkspace();
        var options = workspace
            .Options
            .WithChangedOption(CSharpFormattingOptions.IndentBlock, true);
        
        var nodes = CompletedTypeBuilders
            .Concat(TypeBuilders)
            .Select(typeBuilder => typeBuilder.Node)
            .SelectMany(node => node.DescendantNodes())
            .ToArray();

        return ((SyntaxNode[])
        [
            .. nodes
                .OfType<UsingDirectiveSyntax>()
                .DistinctBy(x => x.ToString())
                .Select(node => node
                    .WithoutTrailingTrivia()),

            .. nodes
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .Select(x => SyntaxFactory
                    .NamespaceDeclaration(
                        x.AttributeLists,
                        x.Modifiers,
                        x.Name,
                        x.Externs,
                        x.Usings,
                        x.Members)
                    )
                .Select(node => Formatter.Format(node, workspace, options))
                .Select(node => node
                    .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed))
        ]).Select(node => node.ToFullString());
    }
}
