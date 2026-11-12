using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class ConversationParticipantResponse
{
    [Display("Participant ID")]
    [JsonProperty("id")]
    public string? ParticipantId { get; set; }

    [Display("Name")]
    [JsonProperty("name")]
    public string? Name { get; set; }

    [Display("Email")]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [Display("Contact ID")]
    [JsonProperty("contact_id")]
    public string? ContactId { get; set; }

    [Display("Account ID")]
    [JsonProperty("account_id")]
    public string? AccountId { get; set; }

    [Display("Company ID")]
    [JsonProperty("organization_id")]
    public string? CompanyId { get; set; }
}
