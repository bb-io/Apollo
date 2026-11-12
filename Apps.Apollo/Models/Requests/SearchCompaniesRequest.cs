using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Requests;

public class SearchCompaniesRequest
{
    [Display("Company name", Description = "Matches part of a company name.")]
    public string? Name { get; set; }

    [Display("Company domains", Description = "Domains or website URLs, up to 1,000.")]
    public IEnumerable<string>? Domains { get; set; }

    [Display("Headquarters locations", Description = "Cities, states or countries, for example New York or Japan.")]
    public IEnumerable<string>? Locations { get; set; }

    [Display("Industries", Description = "Exact Apollo industry names, for example information technology & services.")]
    public IEnumerable<string>? Industries { get; set; }

    [Display("Minimum employees", Description = "Use together with 'Maximum employees'.")]
    public int? MinimumEmployees { get; set; }

    [Display("Maximum employees", Description = "Use together with 'Minimum employees'.")]
    public int? MaximumEmployees { get; set; }

    [Display("Maximum results", Description = "Defaults to 100, maximum 50,000. Each retrieved page of 100 companies consumes one credit.")]
    public int? MaximumResults { get; set; }
}
