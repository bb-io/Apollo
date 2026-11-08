using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo.Actions;
[ActionList("Companies")]
public class CompanyActions(InvocationContext context) : Invocable(context)
{
    [Action("Enrich company", Description = "Find a company using its domain, website or LinkedIn company URL and output available company details. May consume credits.")]
    public async Task<EnrichCompanyResponse> EnrichCompany([ActionParameter] EnrichCompanyRequest input)
    {
        var company = await Client.EnrichCompanyAsync(CompanyEnrichmentMapping.SingleDetails(input));
        if (company != null && string.IsNullOrWhiteSpace(company.CompanyId))
            throw new PluginApplicationException("Apollo returned a company without an ID. Review before syncing the result.");
        return new() { MatchFound = company != null, Company = company };
    }
    [Action("Search companies", Description = "Search company candidates by name, domain and other filters, up to Maximum results. May consume credits per page.")]
    public Task<SearchCompaniesResponse> SearchCompanies([ActionParameter] SearchCompaniesRequest input)
    {
        var (filters, limit) = CompanySearch.Validate(input);
        return Client.SearchCompaniesAsync(filters, limit);
    }
}
