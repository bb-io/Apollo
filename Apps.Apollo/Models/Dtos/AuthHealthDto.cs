using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class AuthHealthDto
{
    [JsonProperty("is_logged_in")]
    public bool IsLoggedIn { get; set; }
}
