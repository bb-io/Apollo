using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class ContactListResponse
{
    [Display("List ID")]
    [JsonProperty("id")]
    public string ListId { get; set; } = string.Empty;

    [Display("List name")]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [Display("Record type")]
    [JsonProperty("modality")]
    public string? Modality { get; set; }

    [Display("Cached contact count", Description = "Count cached by Apollo. Can differ from the current number of contacts.")]
    [JsonProperty("cached_count")]
    public int? CachedCount { get; set; }
}
