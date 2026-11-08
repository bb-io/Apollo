using System.Text.Json;
using Apps.Apollo.Actions;
using Apps.Apollo.Connections;
using Apps.Apollo.Models.Requests;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Integration")]
public class IntegrationTests
{
    private static (InvocationContext Context, AuthenticationCredentialsProvider[] Creds, JsonElement Person) Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Copy appsettings.json.example to appsettings.json and configure local test data.");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? doc.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Set APOLLO_API_KEY or the local api_key setting.");
        AuthenticationCredentialsProvider[] creds = [new("api_key", key!)];
        return (new InvocationContext { AuthenticationCredentialsProviders = creds }, creds, doc.RootElement.GetProperty("TestPerson").Clone());
    }
    [TestMethod]
    public async Task Connection_AcceptsValidKey()
    {
        var config = Load();
        var result = await new ConnectionValidator(config.Context).ValidateConnection(config.Creds, CancellationToken.None);
        Assert.IsTrue(result.IsValid, result.Message);
    }
    [TestMethod]
    public async Task Connection_RejectsInvalidKey()
    {
        var config = Load();
        var result = await new ConnectionValidator(config.Context).ValidateConnection([new("api_key", "deliberately-invalid-blackbird-test-key")], CancellationToken.None);
        Assert.IsFalse(result.IsValid);
    }
    [TestMethod]
    public async Task EnrichPerson_ReturnsExpectedPerson()
    {
        var config = Load();
        var id = config.Person.GetProperty("PersonId").GetString();
        var email = config.Person.GetProperty("Email").GetString();
        var expected = config.Person.GetProperty("ExpectedPersonId").GetString();
        if (string.IsNullOrWhiteSpace(expected) || (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(email)))
            Assert.Inconclusive("Configure Email or PersonId and ExpectedPersonId using a known Apollo record.");
        var result = await new PeopleActions(config.Context).EnrichPerson(new EnrichPersonRequest { PersonId = id, Email = email });
        Assert.IsTrue(result.MatchFound);
        Assert.IsNotNull(result.Person);
        Assert.AreEqual(expected, result.Person.PersonId);
    }
}
