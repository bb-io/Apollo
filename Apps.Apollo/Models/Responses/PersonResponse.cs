using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class PersonResponse
{
    [Display("Person ID")]
    [JsonProperty("id")]
    public string? PersonId { get; set; }

    [Display("Contact ID")]
    [JsonProperty("contact_id")]
    public string? ContactId { get; set; }

    [Display("Full name")]
    [JsonProperty("name")]
    public string? Name { get; set; }

    [Display("First name")]
    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [Display("Last name")]
    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [Display("Email address")]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [Display("Email status")]
    [JsonProperty("email_status")]
    public string? EmailStatus { get; set; }

    [Display("Personal email addresses")]
    [JsonProperty("personal_emails")]
    public List<string> PersonalEmails { get; set; } = [];

    [Display("Job title")]
    [JsonProperty("title")]
    public string? Title { get; set; }

    [Display("LinkedIn URL")]
    [JsonProperty("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [Display("Company ID")]
    [JsonProperty("organization_id")]
    public string? CompanyId { get; set; }

    [Display("Match confidence")]
    [JsonProperty("match_confidence")]
    public string? MatchConfidence { get; set; }

    [Display("City")]
    [JsonProperty("city")]
    public string? City { get; set; }

    [Display("State")]
    [JsonProperty("state")]
    public string? State { get; set; }

    [Display("Country")]
    [JsonProperty("country")]
    public string? Country { get; set; }

    [Display("Company")]
    [JsonProperty("organization")]
    public CompanyResponse? Company { get; set; }
}
