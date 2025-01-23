using System.Reflection;

namespace Metagen.Helpers;

public static class AssemblyExtensions
{
    public static string LoadResource(
        this Assembly assembly,
        string resourceName)
    {
        var resourceStream = assembly
            .GetManifestResourceStream(
                resourceName
            ) ?? throw new InvalidOperationException($"Could not load resource {resourceName}");
        
        using var reader = new StreamReader(resourceStream);
        return reader.ReadToEnd();
    }
}