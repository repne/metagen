using System.Reflection;
using Metagen.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen;

interface IRewriteStuff;

internal sealed record TypeBuilder(
    SyntaxNode Node,
    Func<SyntaxNode, object?[], SyntaxNode>[] Rewriters)
{
    public static TypeBuilder Create<T>()
        => Create(typeof(T));

    public static TypeBuilder Create(Type subjectType)
        => Create(
            // Load file containing our input type from the embedded resource and parse it into a SyntaxTRee
            node: CSharpSyntaxTree
                .ParseText(
                    Assembly
                        .GetExecutingAssembly()
                        .LoadResource(subjectType.FullName + ".cs"))
                .GetRoot()
                ?? throw new InvalidOperationException(
                    $"Could not find the root node for type {subjectType.Name}"),

            // Using reflection, find all methods of non-public nested types implementing IRewriteStuff whose return type is assignable to SyntaxNode
            rewriterMethods: subjectType
                .GetMembers(BindingFlags.NonPublic)
                .OfType<Type>()
                .Single(x => x.IsAssignableTo(typeof(IRewriteStuff)))
                .GetMembers()
                .OfType<MethodInfo>()
                .Where(x => x.ReturnType.IsAssignableTo(typeof(SyntaxNode))));

    private static TypeBuilder Create(SyntaxNode node, IEnumerable<MethodInfo> rewriterMethods)
    {
        // Remove any interface in the node hierarchy that implements IRewriteStuff
        node = node.RemoveNode(
            node
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .Single(
                    interfaceDeclarationSyntax
                        => interfaceDeclarationSyntax
                            .BaseList
                            ?.DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Any(x => x.ToString() == nameof(IRewriteStuff))
                            ?? false),
            SyntaxRemoveOptions.KeepNoTrivia)
        ?? throw new InvalidOperationException();

        return new
        (
            // This is so we can keep a referenced to the target nodes even if they are replaced by changed nodes.
            // Let's say a rewriter renames your input type, the next rewriter wont't be able to find it
            Node: node
                .TrackNodes(node.DescendantNodes()),

            // Creates an array of rewriter lambdas that find and transform specific nodes inside the target type node.
            // Every rewriter lambda will use the parameters to match and call the user defined rewriter
            // SyntaxNode is the target node, object?[] are the user passed paramaters
            // For example: (new RecordDeclarationSyntax(Identifier: "MyType"), [2, "bar"])
            // Will match the method: 
            // RecordDeclarationSyntax MyType(RecordDeclarationSyntax a, int b, string c)
            // Where a will be the SyntaxNode corresponding to the record called MyType
            Rewriters:
            [
                .. rewriterMethods
                    .Select(methodInfo
                        => (Func<SyntaxNode, object?[], SyntaxNode>)((targetNode, args) =>
                        {
                            var originalNode = node
                                .DescendantNodesAndSelf()
                                .SingleOrDefault(node
                                    => node.GetType().IsAssignableTo(methodInfo.ReturnType)
                                    && string.Equals(
                                        node switch
                                        {
                                            TypeDeclarationSyntax n => n.Identifier.ValueText,
                                            MethodDeclarationSyntax n => n.Identifier.ValueText,
                                            PropertyDeclarationSyntax n => n.Identifier.ValueText,
                                            EventDeclarationSyntax n => n.Identifier.ValueText,
                                            ConstructorDeclarationSyntax n => n.Identifier.ValueText,
                                            EnumDeclarationSyntax n => n.Identifier.ValueText,
                                            DelegateDeclarationSyntax n => n.Identifier.ValueText,
                                            ParameterSyntax n => n.Identifier.ValueText,
                                            VariableDeclaratorSyntax n => n.Identifier.ValueText,
                                            FieldDeclarationSyntax n => n.Declaration.Variables[0].Identifier.ValueText,
                                            _ => null
                                        },
                                        methodInfo.Name,
                                        StringComparison.OrdinalIgnoreCase))
                                ?? throw new InvalidOperationException($"Could not find target {methodInfo.Name} of type {methodInfo.ReturnType.Name}");

                            var oldNode = targetNode.GetCurrentNode(originalNode)
                                ?? throw new InvalidOperationException();

                            object?[] newArgs = [oldNode, .. args];

                            if (!methodInfo
                                .GetParameters()
                                .IsAssignableFrom(newArgs))
                            {
                                return targetNode;
                            }

                            var newNode = methodInfo
                                .Invoke(null, newArgs)
                                as SyntaxNode
                                ?? throw new InvalidOperationException();

                            return oldNode != newNode
                                ? targetNode
                                    ?.ReplaceNode(oldNode, newNode)
                                    ?? throw new InvalidOperationException()
                                : targetNode;
                        }))
            ]
        );
    }

    public TypeBuilder Build(object?[] args)
        => this with
        {
            Node = Rewriters
                .Aggregate(
                    Node,
                    (node, rewriter) => rewriter(node, args)
                )
        };
}