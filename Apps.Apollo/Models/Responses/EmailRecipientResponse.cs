using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class EmailRecipientResponse
{
    [Display("Email address")]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [Display("Recipient type")]
    [JsonProperty("type")]
    public string? Type { get; set; }
}
