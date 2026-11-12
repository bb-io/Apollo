using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class ConversationResponse
{
    [Display("Conversation ID")]
    [JsonProperty("id")]
    public string ConversationId { get; set; } = string.Empty;

    [Display("Topic")]
    [JsonProperty("topic")]
    public string? Topic { get; set; }

    [Display("Start time")]
    [JsonProperty("start_time")]
    public DateTime? StartTime { get; set; }

    [Display("Duration", Description = "Conversation duration in seconds.")]
    [JsonProperty("duration")]
    public int? Duration { get; set; }

    [Display("Processing state")]
    [JsonProperty("state")]
    public string? State { get; set; }

    [Display("Host")]
    [JsonProperty("host")]
    public string? Host { get; set; }

    [Display("Host user ID")]
    [JsonProperty("host_id")]
    public string? HostId { get; set; }

    [Display("Conversation type")]
    [JsonProperty("conversation_type")]
    public string? Type { get; set; }

    [Display("Private")]
    [JsonProperty("is_private")]
    public bool? Private { get; set; }

    [Display("Can access conversation")]
    [JsonProperty("can_access_conversation")]
    public bool? CanAccess { get; set; }

    [Display("Account IDs")]
    [JsonProperty("account_ids")]
    public List<string> AccountIds { get; set; } = [];

    [Display("Participant names")]
    [JsonProperty("participant_names")]
    public List<string> ParticipantNames { get; set; } = [];
}
