using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class PersonMatchDto
{
    [JsonProperty("person")]
    public PersonResponse? Person { get; set; }
}
