using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Apps.Apollo.Services;
internal static class CompanySearch
{
    internal static (List<KeyValuePair<string, string>> Filters, int Limit) Validate(SearchCompaniesRequest input)
    {
        var limit = input.MaximumResults ?? 100;
        if (limit < 1 || limit > 50000) throw new PluginMisconfigurationException("Set Maximum results between 1 and 50,000.");
        var filters = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(input.Name)) filters.Add(new("q_organization_name", input.Name.Trim()));
        var domains = Clean(input.Domains).Select(CompanyEnrichmentMapping.Domain).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (domains.Length > 1000) throw new PluginMisconfigurationException("Supply at most 1,000 unique company domains.");
        foreach (var d in domains) filters.Add(new("q_organization_domains_list[]", d));
        foreach (var location in Clean(input.Locations)) filters.Add(new("organization_locations[]", location));
        foreach (var industry in Clean(input.Industries)) filters.Add(new("organization_industries[]", industry));
        if (input.MinimumEmployees.HasValue || input.MaximumEmployees.HasValue)
        {
            if (!input.MinimumEmployees.HasValue || !input.MaximumEmployees.HasValue || input.MinimumEmployees < 0 || input.MaximumEmployees < input.MinimumEmployees)
                throw new PluginMisconfigurationException("Supply both employee bounds, with 0 <= Minimum employees <= Maximum employees.");
            filters.Add(new("organization_num_employees_ranges[]", $"{input.MinimumEmployees.Value},{input.MaximumEmployees.Value}"));
        }
        if (filters.Count == 0) throw new PluginMisconfigurationException("Supply a company name, domain, location, industry or employee range to narrow the search.");
        return (filters, limit);
    }
    private static string[] Clean(string[]? values) => (values ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    internal static CompanySearchPage Parse(string content, int requestedPage)
    {
        try
        {
            var payload = JObject.Parse(content);
            if (payload["organizations"] is not JArray organizations)
                throw new PluginApplicationException("Apollo omitted the company search results. Retry the search.");
            var companies = organizations.ToObject<CompanyResponse?[]>()!;
            if (companies.Any(x => x == null || string.IsNullOrWhiteSpace(x.CompanyId)))
                throw new PluginApplicationException("Apollo returned a search company without an ID. Retry the search.");
            var combined = companies.Select(x => x!).ToList();
            var accounts = payload["accounts"] as JArray;
            foreach (var token in accounts ?? new JArray())
            {
                if (token is not JObject account || string.IsNullOrWhiteSpace((string?)account["id"]))
                    throw new PluginApplicationException("Apollo returned a saved account without an account ID. Retry the search.");
                // Account id identifies a saved workspace record, never the database organization.
                var mapped = (JObject)account.DeepClone();
                mapped["account_id"] = account["id"]?.DeepClone();
                mapped["id"] = account["organization_id"]?.DeepClone();
                if (string.IsNullOrWhiteSpace((string?)mapped["primary_domain"]))
                    mapped["primary_domain"] = account["domain"]?.DeepClone();
                combined.Add(mapped.ToObject<CompanyResponse>()!);
            }
            var pagination = payload["pagination"] as JObject;
            var page = (int?)pagination?["page"];
            if (page.HasValue && page != requestedPage)
                throw new PluginApplicationException("Apollo returned the wrong search page. Stopped to avoid repeating results and credit usage.");
            var totalPages = (int?)pagination?["total_pages"];
            var totalEntries = (long?)pagination?["total_entries"];
            if (totalPages < 0 || totalEntries < 0)
                throw new PluginApplicationException("Apollo returned invalid search pagination.");
            return new(combined.ToArray(), totalEntries, totalPages,
                (bool?)payload["partial_results_only"] == true, accounts?.Count ?? 0);
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable company search results. Retry the search."); }
        catch (FormatException) { throw new PluginApplicationException("Apollo returned unreadable company search pagination. Retry the search."); }
        catch (ArgumentException) { throw new PluginApplicationException("Apollo returned unreadable company search data. Retry the search."); }
    }
    internal static string Identity(CompanyResponse company)
        => !string.IsNullOrWhiteSpace(company.CompanyId) ? "company:" + company.CompanyId
         : !string.IsNullOrWhiteSpace(company.AccountId) ? "account:" + company.AccountId
         : throw new PluginApplicationException("Apollo returned a company without a company or account ID.");
    internal static bool AddCompany(List<CompanyResponse> results, Dictionary<string, CompanyResponse> seen, CompanyResponse company, int limit)
    {
        var key = Identity(company);
        if (seen.TryGetValue(key, out var existing))
        {
            // Keep the organization's profile while retaining its first linked saved-account ID.
            existing.AccountId ??= company.AccountId;
            return false;
        }
        if (results.Count >= limit) return false;
        seen.Add(key, company);
        results.Add(company);
        return true;
    }

}
internal record CompanySearchPage(CompanyResponse[] Companies, long? TotalEntries, int? TotalPages, bool Partial, int AccountCount);
