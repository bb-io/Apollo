using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
namespace Apps.Apollo.Models.Responses;
public class CompanyResponse
{
    [Display("Account ID", Description = "Saved Apollo account ID, when returned. Distinct from Company ID.")] [JsonProperty("account_id")] public string? AccountId { get; set; }
    [Display("Company ID")] [JsonProperty("id")] public string? CompanyId { get; set; }
    [Display("Company name")] [JsonProperty("name")] public string? Name { get; set; }
    [Display("Website URL")] [JsonProperty("website_url")] public string? WebsiteUrl { get; set; }
    [Display("LinkedIn URL")] [JsonProperty("linkedin_url")] public string? LinkedinUrl { get; set; }
    [Display("Company domain")] [JsonProperty("primary_domain")] public string? Domain { get; set; }
    [Display("Industry")] [JsonProperty("industry")] public string? Industry { get; set; }
    [Display("Estimated employees")] [JsonProperty("estimated_num_employees")] public int? EstimatedEmployees { get; set; }
    [Display("Description")] [JsonProperty("short_description")] public string? Description { get; set; }
    [Display("Company phone number")] [JsonProperty("phone")] public string? Phone { get; set; }
    [Display("City")] [JsonProperty("city")] public string? City { get; set; }
    [Display("State")] [JsonProperty("state")] public string? State { get; set; }
    [Display("Country")] [JsonProperty("country")] public string? Country { get; set; }
    [Display("Street address")] [JsonProperty("street_address")] public string? StreetAddress { get; set; }
    [Display("Postal code")] [JsonProperty("postal_code")] public string? PostalCode { get; set; }
    [Display("Founded year")] [JsonProperty("founded_year")] public int? FoundedYear { get; set; }
    [Display("Annual revenue")] [JsonProperty("annual_revenue")] public decimal? AnnualRevenue { get; set; }
    [Display("Annual revenue display")] [JsonProperty("annual_revenue_printed")] public string? AnnualRevenuePrinted { get; set; }
    [Display("Total funding")] [JsonProperty("total_funding")] public decimal? TotalFunding { get; set; }
    [Display("Latest funding stage")] [JsonProperty("latest_funding_stage")] public string? LatestFundingStage { get; set; }
    [Display("Latest funding date")] [JsonProperty("latest_funding_round_date")] public string? LatestFundingDate { get; set; }
    [Display("Parent company ID")] [JsonProperty("owned_by_organization_id")] public string? ParentCompanyId { get; set; }
}
public class EnrichCompanyResponse
{
    [Display("Match found")] public bool MatchFound { get; set; }
    [Display("Company")] public CompanyResponse? Company { get; set; }
}
public class CompanyEnrichmentResult
{
    [Display("Input position", Description = "One-based position in the original Companies collection.")] public int InputPosition { get; set; }
    [Display("Source record ID")] public string? SourceRecordId { get; set; }
    [Display("Requested domain")] public string RequestedDomain { get; set; } = "";
    [Display("Match found")] public bool MatchFound { get; set; }
    [Display("Company")] public CompanyResponse? Company { get; set; }
}
public class EnrichCompaniesResponse
{
    [Display("Results")] public CompanyEnrichmentResult[] Results { get; set; } = [];
    [Display("Requested companies")] public int RequestedCompanies { get; set; }
    [Display("Unique domains")] public int UniqueDomains { get; set; }
    [Display("Matched companies", Description = "Number of matched source rows, including duplicate domains.")] public int MatchedCompanies { get; set; }
    [Display("Unmatched companies")] public int UnmatchedCompanies { get; set; }
}
