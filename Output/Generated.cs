#pragma warning disable CA1822

using Metagen.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metagen.Source
{
    public record ProductRequestInput(Int32 Bla, Int32 Bar)
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
    }
}

namespace Metagen.Source
{
    public record AccountRequestInput()
    {
        public int SumTwoNumbers()
        {
            return (((0 + 42) + 7) + 21);
        }

        public Dictionary<string, object?> ToDictionary()
            => new()
            {
                ["Bla"] = "Bla",
                ["Foo"] = "Bar"
            };

        public void Foo(int lol)
        {
        }
    }
}