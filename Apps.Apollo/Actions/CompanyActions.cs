using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Actions;

[ActionList("Companies")]
public class CompanyActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int MaxDomains = 1000;
    
    [Action("Search companies", Description = "Search companies by name, domain, location, industry or size. Each retrieved page of 100 companies consumes one credit.")]
    public async Task<SearchCompaniesResponse> SearchCompanies([ActionParameter] SearchCompaniesRequest input)
    {
        ValidateSearchCompaniesFilters(input);
        var maximumResults = InputValidator.GetMaximumResults(input.MaximumResults);

        var result = await Client.Paginate<CompaniesPageDto, CompanyResponse>(
            (page, pageSize) => CreateSearchCompaniesRequest(input, page, pageSize),
            x => x.GetCompanies(),
            maximumResults);

        var companies = result.Items.DistinctBy(x => x.CompanyId ?? x.AccountId).ToList();

        return new()
        {
            Companies = companies,
            ReturnedCompanies = companies.Count,
            TotalMatchingRecords = result.TotalEntries
        };
    }

    [Action("Enrich company", Description = "Find a company by its domain, website or LinkedIn company URL and output its details. Consumes one credit.")]
    public async Task<EnrichCompanyResponse> EnrichCompany([ActionParameter] EnrichCompanyRequest input)
    {
        if (new[] { input.Domain, input.Website, input.LinkedinUrl }.All(string.IsNullOrWhiteSpace))
            throw new PluginMisconfigurationException("Please fill in 'Company domain', 'Website URL' or 'LinkedIn company URL'.");

        var request = new RestRequest("organizations/enrich")
            .AddOptionalQueryParameter("domain", input.Domain?.ToBareDomain())
            .AddOptionalQueryParameter("website", input.Website)
            .AddOptionalQueryParameter("linkedin_url", input.LinkedinUrl)
            .AddOptionalQueryParameter("name", input.Name);

        var response = await Client.ExecuteWithErrorHandling<OrganizationEnrichmentDto>(request);
        var matchFound = !string.IsNullOrWhiteSpace(response.Organization?.CompanyId);

        return new()
        {
            MatchFound = matchFound,
            Company = matchFound ? response.Organization : null
        };
    }

    private static void ValidateSearchCompaniesFilters(SearchCompaniesRequest input)
    {
        InputValidator.ValidateBothOrNone(input.MinimumEmployees, input.MaximumEmployees, "Minimum employees", "Maximum employees");
        InputValidator.ValidateRange(input.MinimumEmployees, input.MaximumEmployees, "Minimum employees", "Maximum employees");

        if (input.Domains.CleanValues().Count > MaxDomains)
            throw new PluginMisconfigurationException($"'Company domains' can contain at most {MaxDomains} domains.");

        var hasFilter = !string.IsNullOrWhiteSpace(input.Name)
                        || input.Domains.CleanValues().Count > 0
                        || input.Locations.CleanValues().Count > 0
                        || input.Industries.CleanValues().Count > 0
                        || input.MinimumEmployees.HasValue;

        if (!hasFilter)
            throw new PluginMisconfigurationException("Please fill in at least one filter: company name, domains, locations, industries or employee range.");
    }

    private static RestRequest CreateSearchCompaniesRequest(SearchCompaniesRequest input, int page, int pageSize)
    {
        var request = new RestRequest("mixed_companies/search", Method.Post)
            .AddOptionalQueryParameter("q_organization_name", input.Name)
            .AddQueryParameterList("q_organization_domains_list[]", input.Domains?.Select(x => x.ToBareDomain()))
            .AddQueryParameterList("organization_locations[]", input.Locations)
            .AddQueryParameterList("organization_industries[]", input.Industries)
            .AddPageQueryParameters(page, pageSize);

        if (input.MinimumEmployees.HasValue && input.MaximumEmployees.HasValue)
            request.AddQueryParameter("organization_num_employees_ranges[]", $"{input.MinimumEmployees},{input.MaximumEmployees}");

        return request;
    }
}
