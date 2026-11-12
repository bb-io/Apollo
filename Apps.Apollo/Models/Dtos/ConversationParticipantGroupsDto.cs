using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ConversationParticipantGroupsDto
{
    [JsonProperty("internal")]
    public List<ConversationParticipantResponse> Internal { get; set; } = [];

    [JsonProperty("external")]
    public Dictionary<string, List<ConversationParticipantResponse>> External { get; set; } = [];

    public List<ConversationParticipantResponse> GetAll()
    {
        return Internal.Concat(External.Values.SelectMany(x => x)).ToList();
    }
}
