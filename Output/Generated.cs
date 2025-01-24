using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#pragma warning disable CA1822

using Metagen.Helpers;

namespace Metagen.Source
{
    public class MyApiClient(HttpClient httpClient)
    {
        public async Task<HttpResponseMessage> SaveProductAsync(ProductRequest request)
            => await httpClient.PutAsync(
                request.Endpoint,
                request.ToFormUrlEncodedContent());
    }
}

namespace Metagen.Source
{
    public record ProductRequest(string Id, String Name, Int32 Price)
    {
        public Uri Endpoint { get; } = new("");

        public Dictionary<string, object?> ToDictionary()
            => new()
            {
                ["Name"] = Name,
                ["Price"] = Price
            };

        public FormUrlEncodedContent ToFormUrlEncodedContent()
            => new(
                new Dictionary<string, string>
                {
                    ["Name"] = Name.ToString(),
                    ["Price"] = Price.ToString()
                }
            );

        public int SumTwoNumbers()
        {
            return 0;
        }

        public void Foo(int bar)
        {
        }
    }
}