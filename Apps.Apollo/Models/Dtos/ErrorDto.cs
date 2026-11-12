using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ErrorDto
{
    [JsonProperty("error")]
    public string? Error { get; set; }

    [JsonProperty("error_details")]
    public ErrorDetailsDto? ErrorDetails { get; set; }
}
