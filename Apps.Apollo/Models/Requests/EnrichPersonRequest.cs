using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Requests;
public class PersonIdentityInput
{
    [Display("Person ID", Description = "Apollo database person ID, rather than a saved contact ID.")] public string? PersonId { get; set; }
    [Display("Email address")] public string? Email { get; set; }
    [Display("LinkedIn URL")] public string? LinkedinUrl { get; set; }
    [Display("Full name", Description = "Use with a company domain or company name for a more reliable match.")] public string? Name { get; set; }
    [Display("First name")] public string? FirstName { get; set; }
    [Display("Last name")] public string? LastName { get; set; }
    [Display("Company domain", Description = "For example, apollo.io.")] public string? Domain { get; set; }
    [Display("Company name")] public string? OrganizationName { get; set; }
}
public class EnrichPersonRequest : PersonIdentityInput
{
    [Display("Include personal emails", Description = "Defaults to false. Enrichment may consume Apollo credits.")] public bool? RevealPersonalEmails { get; set; }
}
