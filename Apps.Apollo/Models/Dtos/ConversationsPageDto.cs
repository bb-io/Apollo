using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ConversationsPageDto : PageDto
{
    [JsonProperty("conversations")]
    public List<ConversationResponse> Conversations { get; set; } = [];
}
