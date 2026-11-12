using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class PaginationDto
{
    [JsonProperty("total_entries")]
    public long? TotalEntries { get; set; }

    [JsonProperty("total_pages")]
    public int? TotalPages { get; set; }
}
