using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ContactWrapperDto
{
    [JsonProperty("contact")]
    public ContactResponse Contact { get; set; } = new();
}
