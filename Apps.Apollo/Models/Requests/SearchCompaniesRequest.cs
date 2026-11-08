using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Requests;
public class SearchCompaniesRequest
{
    [Display("Company name", Description = "Matches part of a company name. Review candidates before enriching or syncing.")] public string? Name { get; set; }
    [Display("Company domains", Description = "Optional domains or website URLs, up to 1,000 unique domains.")] public string[]? Domains { get; set; }
    [Display("Headquarters locations", Description = "Cities, states or countries, for example New York or Japan.")] public string[]? Locations { get; set; }
    [Display("Industries", Description = "Exact Apollo industry names, for example information technology & services.")] public string[]? Industries { get; set; }
    [Display("Minimum employees", Description = "Provide both minimum and maximum employees when filtering by size.")] public int? MinimumEmployees { get; set; }
    [Display("Maximum employees")] public int? MaximumEmployees { get; set; }
    [Display("Maximum results", Description = "Defaults to 100; maximum 50,000. Each retrieved page may consume Apollo credits.")] public int? MaximumResults { get; set; }
}
