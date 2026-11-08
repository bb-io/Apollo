using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class CompanySearchTests
{
    [TestMethod]
    public void BlankSearchAndInvalidLimits_AreRejected()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new()));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new() { Name = "Apollo", MaximumResults = 0 }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new() { Name = "Apollo", MaximumResults = 50001 }));
    }
    [TestMethod]
    public void Filters_UseDocumentedQueryNamesAndNormalizedDomains()
    {
        var (filters, limit) = CompanySearch.Validate(new()
        {
            Name = " Apollo ", Domains = ["https://www.apollo.io/about", "APOLLO.IO", " "], Locations = ["Japan"],
            Industries = ["information technology & services"], MinimumEmployees = 10, MaximumEmployees = 100
        });
        Assert.AreEqual(100, limit);
        Assert.AreEqual("Apollo", filters.Single(x => x.Key == "q_organization_name").Value);
        Assert.AreEqual("apollo.io", filters.Single(x => x.Key == "q_organization_domains_list[]").Value);
        Assert.AreEqual("10,100", filters.Single(x => x.Key == "organization_num_employees_ranges[]").Value);
        Assert.AreEqual("Japan", filters.Single(x => x.Key == "organization_locations[]").Value);
        Assert.AreEqual("information technology & services", filters.Single(x => x.Key == "organization_industries[]").Value);
    }
    [TestMethod]
    public void IncompleteOrReversedEmployeeRanges_AreRejected()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new() { MinimumEmployees = 10 }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new() { MinimumEmployees = 100, MaximumEmployees = 10 }));
    }
    [TestMethod]
    public void TooManyDomains_AreRejected()
        => Assert.ThrowsException<PluginMisconfigurationException>(() => CompanySearch.Validate(new() { Domains = Enumerable.Range(1, 1001).Select(i => $"company{i}.example").ToArray() }));
    [TestMethod]
    public void EmptySearchPage_ReturnsEmptyArray()
    {
        var page = CompanySearch.Parse("""{"organizations":[],"accounts":[],"pagination":{"page":1,"total_pages":0,"total_entries":0}}""", 1);
        Assert.AreEqual(0, page.Companies.Length);
        Assert.AreEqual(0L, page.TotalEntries);
    }
    [TestMethod]
    public void PartialResultsAndSavedAccounts_ArePreservedAsMetadata()
    {
        var page = CompanySearch.Parse("""{"organizations":[{"id":"company1","name":"Example"}],"accounts":[{"id":"account1"}],"partial_results_only":true,"pagination":{"page":2,"total_pages":3,"total_entries":201}}""", 2);
        Assert.IsTrue(page.Partial);
        Assert.AreEqual(1, page.AccountCount);
        Assert.AreEqual(201L, page.TotalEntries);
        Assert.AreEqual("company1", page.Companies[0].CompanyId);
    }
    [TestMethod]
    public void WrongPageAndMalformedResults_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => CompanySearch.Parse("""{"organizations":[],"pagination":{"page":1}}""", 2));
        Assert.ThrowsException<PluginApplicationException>(() => CompanySearch.Parse("""{"organizations":[{}]}""", 1));
        Assert.ThrowsException<PluginApplicationException>(() => CompanySearch.Parse("{}", 1));
    }
    [TestMethod]
    public void AccountsOnlyResponse_ReturnsCompaniesWithDistinctIds()
    {
        var page = CompanySearch.Parse("""
        {"organizations":[],"accounts":[{"id":"account-apollo","organization_id":"org-apollo","name":"Apollo","domain":"apollo.io"},{"id":"account-ms","organization_id":null,"name":"Microsoft","primary_domain":"microsoft.com"}],"pagination":{"page":1,"total_pages":1,"total_entries":2}}
        """, 1);
        Assert.AreEqual(2, page.Companies.Length);
        Assert.AreEqual("org-apollo", page.Companies[0].CompanyId);
        Assert.AreEqual("account-apollo", page.Companies[0].AccountId);
        Assert.AreEqual("apollo.io", page.Companies[0].Domain);
        Assert.IsNull(page.Companies[1].CompanyId);
        Assert.AreEqual("account-ms", page.Companies[1].AccountId);
        Assert.AreEqual("microsoft.com", page.Companies[1].Domain);
    }
    [TestMethod]
    public void OrganizationAndSavedAccount_MergeByOrganizationId()
    {
        var page = CompanySearch.Parse("""
        {"organizations":[{"id":"org-1","name":"Apollo","primary_domain":"apollo.io"}],"accounts":[{"id":"account-1","organization_id":"org-1","domain":"apollo.io"}]}
        """, 1);
        var results = new List<Apps.Apollo.Models.Responses.CompanyResponse>();
        var seen = new Dictionary<string, Apps.Apollo.Models.Responses.CompanyResponse>();
        foreach (var company in page.Companies) CompanySearch.AddCompany(results, seen, company, 10);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("org-1", results[0].CompanyId);
        Assert.AreEqual("account-1", results[0].AccountId);
    }
    [TestMethod]
    public void AccountAndCompanyIds_DoNotCollideWhenValuesMatch()
    {
        var results = new List<Apps.Apollo.Models.Responses.CompanyResponse>();
        var seen = new Dictionary<string, Apps.Apollo.Models.Responses.CompanyResponse>();
        CompanySearch.AddCompany(results, seen, new() { CompanyId = "same" }, 10);
        CompanySearch.AddCompany(results, seen, new() { AccountId = "same" }, 10);
        Assert.AreEqual(2, results.Count);
    }

}
[TestClass]
[TestCategory("Integration")]
public class CompanySearchIntegrationTests
{
    private static CompanyActions Actions()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Configure your local API key as for the other live tests.");
        using var settings = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? settings.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Set your local Apollo API key.");
        return new(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", key!) } });
    }
    [TestMethod]
    public async Task SearchCompanies_ByNameReturnsCandidates()
    {
        var result = await Actions().SearchCompanies(new() { Name = "Microsoft", MaximumResults = 5 });
        Assert.IsTrue(result.ReturnedCompanies > 0);
        Assert.IsTrue(result.ReturnedCompanies <= 5);
        Assert.AreEqual(1, result.PagesRetrieved);
        Assert.IsTrue(result.Companies.All(c => (!string.IsNullOrWhiteSpace(c.CompanyId) || !string.IsNullOrWhiteSpace(c.AccountId))));
        Assert.IsTrue(result.Companies.All(c => c.Name?.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) == true));
    }
    [TestMethod]
    public async Task SearchCompanies_ByDomainsReturnsKnownCompanies()
    {
        var result = await Actions().SearchCompanies(new() { Domains = ["https://www.apollo.io/", "microsoft.com"], MaximumResults = 10 });
        var diagnostic = $"Returned={result.ReturnedCompanies}; Total={result.TotalMatchingRecords}; Pages={result.PagesRetrieved}; Limited={result.ResultsLimited}; SavedAccountsIncluded={result.SavedAccountsIncluded}; Domains=[{string.Join(", ", result.Companies.Select(c => c.Domain ?? "<blank>"))}]";
        Console.WriteLine(diagnostic);
        Assert.IsTrue(result.Companies.Any(c => c.Domain == "apollo.io"), "Expected apollo.io. " + diagnostic);
        Assert.IsTrue(result.Companies.Any(c => c.Domain == "microsoft.com"), "Expected microsoft.com. " + diagnostic);
        Assert.IsTrue(result.ReturnedCompanies <= 10);
        Assert.AreEqual(1, result.PagesRetrieved);
    }
    [TestMethod]
    public async Task SearchCompanies_PaginatesToMaximumResults()
    {
        var result = await Actions().SearchCompanies(new() { Name = "software", MaximumResults = 101 });
        Assert.AreEqual(2, result.PagesRetrieved);
        Assert.AreEqual(101, result.ReturnedCompanies);
        Assert.AreEqual(101, result.Companies.Select(CompanySearch.Identity).Distinct().Count());
        Assert.IsTrue(result.ResultsLimited);
    }
}
