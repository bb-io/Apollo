using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class ContactPhoneResponse
{
    [Display("Phone number")]
    [JsonProperty("sanitized_number")]
    public string? Number { get; set; }

    [Display("Raw phone number")]
    [JsonProperty("raw_number")]
    public string? RawNumber { get; set; }

    [Display("Phone type")]
    [JsonProperty("type")]
    public string? Type { get; set; }

    [Display("Phone status")]
    [JsonProperty("status")]
    public string? Status { get; set; }

    [Display("Do not call status")]
    [JsonProperty("dnc_status")]
    public string? DoNotCallStatus { get; set; }
}
