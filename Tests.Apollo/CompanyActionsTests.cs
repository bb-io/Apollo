using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class CompanyActionsTests : TestBase
{
    [TestMethod]
    public async Task EnrichCompany_ByDomain_ReturnsCompany()
    {
        var actions = new CompanyActions(InvocationContext);

        var result = await actions.EnrichCompany(new EnrichCompanyRequest { Domain = "https://www.apollo.io/" });

        PrintJsonResult(result);
        Assert.IsTrue(result.MatchFound);
        Assert.AreEqual("apollo.io", result.Company?.Domain);
    }

    [TestMethod]
    public async Task SearchCompanies_ByName_ReturnsCompanies()
    {
        var actions = new CompanyActions(InvocationContext);

        var result = await actions.SearchCompanies(new SearchCompaniesRequest
        {
            Name = "Apollo",
            MinimumEmployees = 100,
            MaximumEmployees = 10000,
            MaximumResults = 5
        });

        PrintJsonResult(result);
        Assert.IsTrue(result.ReturnedCompanies > 0);
    }
}
