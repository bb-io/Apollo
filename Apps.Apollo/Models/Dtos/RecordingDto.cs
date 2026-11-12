using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class RecordingDto
{
    [JsonProperty("url")]
    public string? Url { get; set; }
}
