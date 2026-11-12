using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class ContactResponse
{
    [Display("Contact ID")]
    [JsonProperty("id")]
    public string ContactId { get; set; } = string.Empty;

    [Display("Person ID")]
    [JsonProperty("person_id")]
    public string? PersonId { get; set; }

    [Display("Full name")]
    [JsonProperty("name")]
    public string? Name { get; set; }

    [Display("First name")]
    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [Display("Last name")]
    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [Display("Job title")]
    [JsonProperty("title")]
    public string? Title { get; set; }

    [Display("Email address")]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [Display("Email status")]
    [JsonProperty("email_status")]
    public string? EmailStatus { get; set; }

    [Display("Email unsubscribed")]
    [JsonProperty("email_unsubscribed")]
    public bool? EmailUnsubscribed { get; set; }

    [Display("Company name")]
    [JsonProperty("organization_name")]
    public string? CompanyName { get; set; }

    [Display("Company ID")]
    [JsonProperty("organization_id")]
    public string? CompanyId { get; set; }

    [Display("Account ID")]
    [JsonProperty("account_id")]
    public string? AccountId { get; set; }

    [Display("LinkedIn URL")]
    [JsonProperty("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [Display("Phone number")]
    [JsonProperty("sanitized_phone")]
    public string? Phone { get; set; }

    [Display("Phone numbers")]
    [JsonProperty("phone_numbers")]
    public List<ContactPhoneResponse> PhoneNumbers { get; set; } = [];

    [Display("City")]
    [JsonProperty("city")]
    public string? City { get; set; }

    [Display("State")]
    [JsonProperty("state")]
    public string? State { get; set; }

    [Display("Country")]
    [JsonProperty("country")]
    public string? Country { get; set; }

    [Display("Address")]
    [JsonProperty("present_raw_address")]
    public string? Address { get; set; }

    [Display("Contact stage ID")]
    [JsonProperty("contact_stage_id")]
    public string? StageId { get; set; }

    [Display("Owner ID")]
    [JsonProperty("owner_id")]
    public string? OwnerId { get; set; }

    [Display("List IDs")]
    [JsonProperty("label_ids")]
    public List<string> ListIds { get; set; } = [];

    [Display("Created at")]
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Updated at")]
    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
