using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class CallsPageDto : PageDto
{
    [JsonProperty("phone_calls")]
    public List<CallResponse> Calls { get; set; } = [];
}
