using Metagen.Source;

var document = new Compilation();

document.Start(typeof(RequestInput));

document = document.Broadcast("Product");
document = document.Broadcast([typeof(int), "Bla"]);
document = document.Broadcast([typeof(int), "Bar"]);

document.Complete();

document.Start(typeof(RequestInput));

document = document.Broadcast("Account");

document = document.Broadcast([42]);
document = document.Broadcast([7]);
document = document.Broadcast([21]);

document = document.Broadcast(["Bla", "Bla"]);
document = document.Broadcast(["Foo", "Bar"]);

document.Complete();

WriteResult(
    filename: "Output/Generated.cs",
    lines: document
        .ToFullStrings());

await Task
    .Delay(-1)
    .ConfigureAwait(false);

static void WriteResult(string filename, IEnumerable<string> lines)
{
    var text = string.Join(Environment.NewLine, lines);

    if (File.Exists(filename))
    {
        File.Delete(filename);
    }

    File.WriteAllText(filename, text);
}
