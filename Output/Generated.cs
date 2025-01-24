#pragma warning disable CA1822

using Metagen.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen.Source
{
    public record ProductRequest(string Id, String Name, Int32 Price)
    {
        public int SumTwoNumbers()
        {
            return 0;
        }

        public Dictionary<string, object?> ToDictionary()
            => new()
            {
                ["Name"] = Name,
                ["Price"] = Price
            };

        public void Foo(int bar)
        {
        }
    }
}