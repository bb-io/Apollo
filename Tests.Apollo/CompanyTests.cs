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
    public async Task EmptyCompanies_IsRejectedBeforeNetworkCall()
        => await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichCompanies(new()));
    [TestMethod]
    public async Task InvalidLaterCompany_IsRejectedBeforeFirstRequest()
    {
        var error = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichCompanies(new()
        {
            Companies = Enumerable.Range(0, 11).Select(i => new CompanyEnrichmentInput { Domain = i == 10 ? "bad@domain.com" : "example.com" }).ToArray()
        }));
        StringAssert.Contains(error.Message, "position 11");
    }
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
    public void ShuffledSparseResponse_RestoresDuplicatesAndMissingRows()
    {
        var companies = CompanyEnrichmentMapping.IndexBatch(["a.com", "b.com", "unknown.invalid"],
            [new() { CompanyId = "b", Domain = "b.com" }, null, new() { CompanyId = "a", Domain = "a.com" }]);
        var domains = new[] { "a.com", "unknown.invalid", "b.com", "a.com" };
        var inputs = domains.Select((domain, i) => new CompanyEnrichmentInput { Domain = domain, SourceRecordId = $"row-{i + 1}" }).ToArray();
        var result = CompanyEnrichmentMapping.RestoreRows(inputs, domains, companies);
        Assert.AreEqual(4, result.RequestedCompanies);
        Assert.AreEqual(3, result.UniqueDomains);
        Assert.AreEqual(3, result.MatchedCompanies);
        Assert.AreEqual(1, result.UnmatchedCompanies);
        Assert.IsNull(result.Results[1].Company);
        Assert.IsFalse(result.Results[1].MatchFound);
        Assert.AreEqual("b", result.Results[2].Company?.CompanyId);
        Assert.AreEqual("a", result.Results[3].Company?.CompanyId);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result.Results.Select(x => x.InputPosition).ToArray());
        CollectionAssert.AreEqual(new[] { "row-1", "row-2", "row-3", "row-4" }, result.Results.Select(x => x.SourceRecordId).ToArray());
    }
    [TestMethod]
    public void AliasesAndConflictingCompanies_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => CompanyEnrichmentMapping.IndexBatch(["old.com"], [new() { CompanyId = "1", Domain = "new.com" }]));
        Assert.ThrowsException<PluginApplicationException>(() => CompanyEnrichmentMapping.IndexBatch(["a.com"], [new() { CompanyId = "1", Domain = "a.com" }, new() { CompanyId = "2", Domain = "a.com" }]));
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
    [TestMethod]
    public async Task EnrichCompanies_PreservesDuplicatesAndUnmatchedDomains()
    {
        string[] domains = ["apollo.io", "blackbird-company-test-unknown.invalid", "microsoft.com", "www.apollo.io"];
        var result = await Actions().EnrichCompanies(new() { Companies = domains.Select((d, i) => new CompanyEnrichmentInput { Domain = d, SourceRecordId = $"row-{i + 1}" }).ToArray() });
        Assert.AreEqual(4, result.RequestedCompanies);
        Assert.AreEqual(3, result.UniqueDomains);
        Assert.AreEqual(3, result.MatchedCompanies);
        Assert.AreEqual(1, result.UnmatchedCompanies);
        Assert.IsFalse(result.Results[1].MatchFound);
        Assert.IsNull(result.Results[1].Company);
        Assert.AreEqual("apollo.io", result.Results[0].Company?.Domain);
        Assert.AreEqual("microsoft.com", result.Results[2].Company?.Domain);
        Assert.AreEqual(result.Results[0].Company?.CompanyId, result.Results[3].Company?.CompanyId);
        CollectionAssert.AreEqual(new[] { "row-1", "row-2", "row-3", "row-4" }, result.Results.Select(x => x.SourceRecordId).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, result.Results.Select(x => x.InputPosition).ToArray());
    }
    [TestMethod]
    public async Task EnrichCompanies_SplitsElevenUniqueDomains()
    {
        var domains = new[] { "apollo.io" }.Concat(Enumerable.Range(1, 9).Select(i => $"blackbird-company-test-{i}.invalid")).Append("microsoft.com").ToArray();
        var result = await Actions().EnrichCompanies(new() { Companies = domains.Select(d => new CompanyEnrichmentInput { Domain = d }).ToArray() });
        Assert.AreEqual(11, result.UniqueDomains);
        Assert.AreEqual(11, result.Results.Length);
        Assert.AreEqual(2, result.MatchedCompanies);
        Assert.AreEqual(9, result.UnmatchedCompanies);
        Assert.AreEqual("apollo.io", result.Results[0].Company?.Domain);
        Assert.AreEqual("microsoft.com", result.Results[10].Company?.Domain);
        Assert.AreEqual(11, result.Results[10].InputPosition);
    }
}
