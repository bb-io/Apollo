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
    [Action("Enrich company", Description = "Find a company using its domain, website or LinkedIn company URL. May consume Apollo credits.")]
    public async Task<EnrichCompanyResponse> EnrichCompany([ActionParameter] EnrichCompanyRequest input)
    {
        var company = await Client.EnrichCompanyAsync(CompanyEnrichmentMapping.SingleDetails(input));
        if (company != null && string.IsNullOrWhiteSpace(company.CompanyId))
            throw new PluginApplicationException("Apollo returned a company without an ID. Review before syncing the result.");
        return new() { MatchFound = company != null, Company = company };
    }
    [Action("Enrich companies", Description = "Enrich companies by domain in batches of ten. Retains source rows and enriches repeated domains once. May consume Apollo credits.")]
    public async Task<EnrichCompaniesResponse> EnrichCompanies([ActionParameter] EnrichCompaniesRequest input)
    {
        if (input.Companies == null || input.Companies.Length == 0)
            throw new PluginMisconfigurationException("Add at least one company to the Companies collection.");
        var domains = new string[input.Companies.Length];
        for (var i = 0; i < domains.Length; i++)
        {
            try { domains[i] = CompanyEnrichmentMapping.Domain(input.Companies[i]?.Domain); }
            catch (PluginMisconfigurationException ex) { throw new PluginMisconfigurationException($"Company at input position {i + 1}: {ex.Message}"); }
        }
        var unique = domains.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var companies = new Dictionary<string, CompanyResponse>(StringComparer.OrdinalIgnoreCase);
        for (var offset = 0; offset < unique.Length; offset += 10)
        {
            var batch = unique.Skip(offset).Take(10).ToArray();
            try
            {
                var response = await Client.EnrichCompaniesAsync(batch);
                foreach (var (key, company) in CompanyEnrichmentMapping.IndexBatch(batch, response)) companies.Add(key, company);
            }
            catch (PluginMisconfigurationException ex)
            { throw new PluginMisconfigurationException($"Company batch {offset / 10 + 1} failed after {offset} unique domains completed. Earlier requests may have consumed credits. Fix the cause before retrying. {ex.Message}"); }
            catch (PluginApplicationException ex)
            { throw new PluginApplicationException($"Company batch {offset / 10 + 1} failed after {offset} unique domains completed. Earlier and current requests may have consumed credits. Review before rerunning. {ex.Message}"); }
        }
        return CompanyEnrichmentMapping.RestoreRows(input.Companies, domains, companies);
    }
    [Action("Search companies", Description = "Search Apollo's company database by name, domain and other filters. Retrieves pages automatically up to Maximum results. May consume credits per page.")]
    public Task<SearchCompaniesResponse> SearchCompanies([ActionParameter] SearchCompaniesRequest input)
    {
        var (filters, limit) = CompanySearch.Validate(input);
        return Client.SearchCompaniesAsync(filters, limit);
    }
}
