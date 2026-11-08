using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Text.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class CompanyTests
{
    private static CompanyActions Actions() => new(new InvocationContext
    { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", "offline-unused-key") } });
    [TestMethod]
    public async Task CompanyNameAlone_IsRejectedBeforeNetworkCall()
        => await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichCompany(new() { Name = "Apollo" }));
    [TestMethod]
    public void Domains_NormalizeWebsitesAndRejectEmails()
    {
        Assert.AreEqual("example.com", CompanyEnrichmentMapping.Domain(" https://WWW.Example.COM/about "));
        Assert.AreEqual("example.com", CompanyEnrichmentMapping.Domain("EXAMPLE.com."));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanyEnrichmentMapping.Domain("me@example.com"));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanyEnrichmentMapping.Domain("https://127.0.0.1"));
    }
    [TestMethod]
    public void WebsiteAndLinkedin_AreMappedToDocumentedParameters()
    {
        var details = CompanyEnrichmentMapping.SingleDetails(new() { Website = "https://example.com", LinkedinUrl = "https://www.linkedin.com/company/example", Name = " Example " });
        Assert.AreEqual("https://example.com/", details["website"]);
        Assert.AreEqual("https://www.linkedin.com/company/example", details["linkedin_url"]);
        Assert.AreEqual("Example", details["name"]);
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanyEnrichmentMapping.SingleDetails(new() { LinkedinUrl = "https://linkedin.com/in/person" }));
    }
    [TestMethod]
    public void CompanyResponse_MapsRevenueLocationAndParentId()
    {
        var company = JsonConvert.DeserializeObject<CompanyResponse>("""
        {"id":"c1","primary_domain":"example.com","annual_revenue":1234567.50,"city":"New York","estimated_num_employees":42,"owned_by_organization_id":"parent1","founded_year":2000}
        """)!;
        Assert.AreEqual(1234567.50m, company.AnnualRevenue);
        Assert.AreEqual("parent1", company.ParentCompanyId);
        Assert.AreEqual(42, company.EstimatedEmployees);
        Assert.AreEqual("New York", company.City);
    }
}
[TestClass]
[TestCategory("Integration")]
public class CompanyIntegrationTests
{
    private static CompanyActions Actions()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Configure local appsettings.json as for the person tests.");
        using var settings = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? settings.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Set your local Apollo API key.");
        return new(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", key!) } });
    }
    [TestMethod]
    public async Task EnrichCompany_ReturnsApollo()
    {
        var result = await Actions().EnrichCompany(new() { Domain = "https://www.apollo.io/" });
        Assert.IsTrue(result.MatchFound);
        Assert.IsNotNull(result.Company);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Company.CompanyId));
        Assert.AreEqual("apollo.io", result.Company.Domain?.ToLowerInvariant());
    }
}
