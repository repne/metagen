Assume you have a record:
```csharp
record MyApiRequestDto(string Id);
```

and a web api endpoints definition like

```csharp
List<(
    string Name,
    string Endpoint,
    List<(Type Type, string Name)> Parameters
)> endpoints = [
    (
        Name: "Product",
        Endpoint: "/products",
        Parameters: [
            (typeof(string), "Name"),
            (typeof(int), "Price")
        ]
    )
];
```

add a nested type to your class

```csharp
record MyApiRequestDto(string Id)
{
    private interface IRewriteMyApiRequestDto : IRewriteStuff
    {
    }
}
```

that renames the record using Roslyn when it receives a string

```csharp
record MyApiRequestDto(string Id)
{
    private interface IRewriteMyApiRequestDto : IRewriteStuff
    {
        static TypeDeclarationSyntax MyApiRequestDto(       // Name must match the identifier of the node we want to change
            TypeDeclarationSyntax typeDeclarationSyntax,    // First parameter must match the type of the node we want to change
            string name)
            => typeDeclarationSyntax
                .WithIdentifier(SyntaxFactory.Identifier($"{name}Request"));
    }
}
```

then create a compilation and loop over the endpoints

```csharp
var document = new Compilation();

foreach (var (Name, Endpoint, Parameters) in endpoints)
{
    document.StartType(typeof(MyApiRequestDto));
    document.Broadcast(Name);
    document.CompleteType();
}
```

and save the result to file

```csharp
WriteResult(
    filename: "Output/Generated.cs",
    lines: document
        .ToFullStrings());

static void WriteResult(string filename, IEnumerable<string> lines)
{
    var text = string.Join(Environment.NewLine, lines);

    if (File.Exists(filename))
    {
        File.Delete(filename);
    }

    File.WriteAllText(filename, text);
}
```

the resulting file will be

```csharp
    record ProductRequest(string Id)
    {
    }
```

so let's add a rewriter for the constructor parameters

```csharp
...
static TypeDeclarationSyntax MyApiRequestDto(       // Matching the same node again
    TypeDeclarationSyntax typeDeclarationSyntax,
    Type type,      // the type of the parameter
    string name)    // and its name
    => typeDeclarationSyntax
        .AddParameter(type.Name, name);
...
```

and loop over the parameters as well

```csharp
...
    foreach (var parameter in Parameters)
    {
        document.Broadcast(parameter.Type, parameter.Name);
    }
...
```

the resulting Generated.cs file will be

```csharp
record ProductRequest(string Id, String Name, Int32 Price)
{
}
```

let's add a new method to our original input type:

```csharp
...
    public Dictionary<string, object?> ToDictionary()
        => new()
        {
        };
...
```

and a new rewriter method

```csharp
...
        static MethodDeclarationSyntax ToDictionary(
            MethodDeclarationSyntax methodDeclarationSyntax, // We look for a method this time
            Type _, // but we listen to the same Type/String parameters as before
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
...
```

without changing the loop, we rerun the app. the resulting file will be

```csharp
record ProductRequest(string Id, String Name, Int32 Price)
{
    public Dictionary<string, object?> ToDictionary()
        => new()
        {
            ["Name"] = Name,
            ["Price"] = Price
        };
}
```

so the full input object is

```csharp
public record MyApiRequestDto(string Id)
{
    public Dictionary<string, object?> ToDictionary()
        => new()
        {
        };

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
    }
}
```

the feed loop is

```csharp
List<(string Name, string Endpoint, List<(Type Type, string Name)> Parameters)> endpoints = [
    (
        Name: "Product",
        Endpoint: "/products",
        Parameters: [
            (typeof(string), "Name"),
            (typeof(int), "Price")
        ]
    )
];

var document = new Compilation();

foreach (var (Name, Endpoint, Parameters) in endpoints)
{
    document.StartType(typeof(MyApiRequestDto));

    document.Broadcast(Name);

    foreach (var parameter in Parameters)
    {
        document.Broadcast(parameter.Type, parameter.Name);
    }

    document.CompleteType();
}
```

and the resulting file

```csharp
public record ProductRequest(string Id, String Name, Int32 Price)
{
    public Dictionary<string, object?> ToDictionary()
        => new()
        {
            ["Name"] = Name,
            ["Price"] = Price
        };
}
```