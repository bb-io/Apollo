using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
namespace Apps.Apollo.Models.Responses;
public class ContactResponse
{
    [Display("Contact ID")] [JsonProperty("id")] public string? ContactId { get; set; }
    [Display("Person ID")] [JsonProperty("person_id")] public string? PersonId { get; set; }
    [Display("Full name")] [JsonProperty("name")] public string? Name { get; set; }
    [Display("First name")] [JsonProperty("first_name")] public string? FirstName { get; set; }
    [Display("Last name")] [JsonProperty("last_name")] public string? LastName { get; set; }
    [Display("Job title")] [JsonProperty("title")] public string? Title { get; set; }
    [Display("Email address")] [JsonProperty("email")] public string? Email { get; set; }
    [Display("Email status")] [JsonProperty("email_status")] public string? EmailStatus { get; set; }
    [Display("Company name")] [JsonProperty("organization_name")] public string? CompanyName { get; set; }
    [Display("Company ID")] [JsonProperty("organization_id")] public string? CompanyId { get; set; }
    [Display("Account ID")] [JsonProperty("account_id")] public string? AccountId { get; set; }
    [Display("LinkedIn URL")] [JsonProperty("linkedin_url")] public string? LinkedinUrl { get; set; }
    [Display("Phone number")] [JsonProperty("sanitized_phone")] public string? Phone { get; set; }
    [Display("City")] [JsonProperty("city")] public string? City { get; set; }
    [Display("State")] [JsonProperty("state")] public string? State { get; set; }
    [Display("Country")] [JsonProperty("country")] public string? Country { get; set; }
    [Display("Address")] [JsonProperty("present_raw_address")] public string? Address { get; set; }
    [Display("Contact stage ID")] [JsonProperty("contact_stage_id")] public string? StageId { get; set; }
    [Display("Owner ID")] [JsonProperty("owner_id")] public string? OwnerId { get; set; }
    [Display("Created at")] [JsonProperty("created_at")] public string? CreatedAt { get; set; }
    [Display("Updated at")] [JsonProperty("updated_at")] public string? UpdatedAt { get; set; }
    [Display("List IDs")] [JsonProperty("label_ids")] public string[] ListIds { get; set; } = [];
    [Display("Phone numbers")] [JsonProperty("phone_numbers")] public ContactPhoneResponse[] PhoneNumbers { get; set; } = [];
    [Display("Email unsubscribed")] [JsonProperty("email_unsubscribed")] public bool? EmailUnsubscribed { get; set; }
}
public class ContactPhoneResponse
{
    [Display("Phone number")] [JsonProperty("sanitized_number")] public string? Number { get; set; }
    [Display("Raw phone number")] [JsonProperty("raw_number")] public string? RawNumber { get; set; }
    [Display("Phone type")] [JsonProperty("type")] public string? Type { get; set; }
    [Display("Phone status")] [JsonProperty("status")] public string? Status { get; set; }
    [Display("Do not call status")] [JsonProperty("dnc_status")] public string? DoNotCallStatus { get; set; }
}
public class SearchContactsResponse
{
    [Display("Contacts")] public ContactResponse[] Contacts { get; set; } = [];
    [Display("Returned contacts")] public int ReturnedContacts { get; set; }
    [Display("Total matching contacts")] public long? TotalMatchingContacts { get; set; }
    [Display("Pages retrieved")] public int PagesRetrieved { get; set; }
    [Display("Results limited", Description = "More contacts may exist beyond the result limit, page budget or Apollo access limits.")] public bool ResultsLimited { get; set; }
}
public class ContactListResponse
{
    [Display("List ID")] [JsonProperty("id")] public string ListId { get; set; } = "";
    [Display("List name")] [JsonProperty("name")] public string Name { get; set; } = "";
    [Display("Record type")] [JsonProperty("modality")] public string? Modality { get; set; }
    [Display("Cached contact count", Description = "Apollo's cached count; may differ from the current number of contacts.")] [JsonProperty("cached_count")] public int? CachedCount { get; set; }
}
public class SearchContactListsResponse
{
    [Display("Contact lists")] public ContactListResponse[] Lists { get; set; } = [];
}
