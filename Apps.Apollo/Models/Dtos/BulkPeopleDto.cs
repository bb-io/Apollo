using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;
namespace Apps.Apollo.Models.Dtos;
public class BulkPeopleDto
{
    [JsonProperty("status")] public string? Status { get; set; }
    [JsonProperty("total_requested_enrichments")] public int? TotalRequested { get; set; }
    [JsonProperty("credits_consumed")] public decimal? CreditsConsumed { get; set; }
    [JsonProperty("matches")] public PersonResponse?[]? Matches { get; set; }
}
