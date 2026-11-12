using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class PageDto
{
    [JsonProperty("pagination")]
    public PaginationDto? Pagination { get; set; }
}
