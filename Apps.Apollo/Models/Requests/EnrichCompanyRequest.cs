using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Requests;
public class EnrichCompanyRequest
{
    [Display("Company domain", Description = "For example, apollo.io. A website URL is also accepted and normalized.")] public string? Domain { get; set; }
    [Display("Website URL")] public string? Website { get; set; }
    [Display("LinkedIn company URL")] public string? LinkedinUrl { get; set; }
    [Display("Company name", Description = "Optional matching hint. Also provide a domain, website URL or LinkedIn company URL.")] public string? Name { get; set; }
}
