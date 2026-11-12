using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class OutreachEmailRecipientResponse
{
    [Display("Email address")]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [Display("Recipient type")]
    [JsonProperty("recipient_type_cd")]
    public string? Type { get; set; }
}
