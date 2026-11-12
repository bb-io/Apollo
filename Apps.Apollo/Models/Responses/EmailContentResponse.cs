using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class EmailContentResponse
{
    [Display("Email message ID")]
    [JsonProperty("id")]
    public string MessageId { get; set; } = string.Empty;

    [Display("Subject")]
    [JsonProperty("subject")]
    public string? Subject { get; set; }

    [Display("Body")]
    [JsonProperty("body")]
    public string? Body { get; set; }

    [Display("Body format")]
    [JsonProperty("body_format")]
    public string? BodyFormat { get; set; }

    [Display("Status")]
    [JsonProperty("status")]
    public string? Status { get; set; }

    [Display("Sent at")]
    [JsonProperty("sent_at")]
    public DateTime? SentAt { get; set; }

    [Display("Recipients")]
    [JsonProperty("recipients")]
    public List<EmailRecipientResponse> Recipients { get; set; } = [];
}
