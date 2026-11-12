using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ErrorDetailsDto
{
    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }
}
