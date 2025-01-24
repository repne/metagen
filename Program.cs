using Metagen.Source;

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

WriteResult(
    filename: "Output/Generated.cs",
    lines: document
        .ToFullStrings());

// await Task
//     .Delay(-1)
//     .ConfigureAwait(false);

static void WriteResult(string filename, IEnumerable<string> lines)
{
    var text = string.Join(Environment.NewLine, lines);

    if (File.Exists(filename))
    {
        File.Delete(filename);
    }

    File.WriteAllText(filename, text);
}
