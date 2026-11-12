using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class EmailContentListDto
{
    [JsonProperty("emailer_messages")]
    public List<EmailContentResponse> Emails { get; set; } = [];
}
