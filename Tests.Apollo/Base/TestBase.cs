using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Tests.Apollo.Base;

public class TestBase
{
    protected IEnumerable<AuthenticationCredentialsProvider> Creds { get; }

    protected InvocationContext InvocationContext { get; }

    protected TestBase()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        Creds = config
            .GetSection("ConnectionDefinition")
            .GetChildren()
            .Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value ?? string.Empty))
            .ToList();

        InvocationContext = new InvocationContext
        {
            AuthenticationCredentialsProviders = Creds
        };
    }

    protected static void PrintJsonResult(object result)
    {
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }

    protected static void PrintDataHandlerResult(IEnumerable<DataSourceItem> items)
    {
        var itemList = items.ToList();
        Console.WriteLine($"Count: {itemList.Count}");

        foreach (var item in itemList)
            Console.WriteLine($"{item.Value} - {item.DisplayName}");
    }
}
