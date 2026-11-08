using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Requests;
public class EnrichCompanyRequest
{
    [Display("Company domain", Description = "For example, apollo.io. A website URL is also accepted and normalized.")] public string? Domain { get; set; }
    [Display("Website URL")] public string? Website { get; set; }
    [Display("LinkedIn company URL")] public string? LinkedinUrl { get; set; }
    [Display("Company name", Description = "Optional matching hint. Also provide a domain, website URL or LinkedIn company URL.")] public string? Name { get; set; }
}
public class EnrichCompaniesRequest
{
    [Display("Companies", Description = "Companies identified by domain. Repeated domains are enriched once per action and restored to their original rows.")]
    public CompanyEnrichmentInput[] Companies { get; set; } = [];
}
public class CompanyEnrichmentInput
{
    [Display("Company domain", Description = "For example, apollo.io. A website URL is accepted and normalized.")] public string Domain { get; set; } = "";
    [Display("Source record ID", Description = "External record ID returned unchanged; never sent to Apollo.")] public string? SourceRecordId { get; set; }
}
