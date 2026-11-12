using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ConversationDetailsDto : ConversationResponse
{
    [JsonProperty("audio_recording")]
    public RecordingDto? AudioRecording { get; set; }

    [JsonProperty("video_recording")]
    public RecordingDto? VideoRecording { get; set; }

    [JsonProperty("transcript")]
    public List<TranscriptSegmentResponse> Transcript { get; set; } = [];

    [JsonProperty("participants")]
    public ConversationParticipantGroupsDto? Participants { get; set; }

    [JsonProperty("opportunity_ids")]
    public List<string> OpportunityIds { get; set; } = [];

    [JsonProperty("failure_code")]
    public string? FailureCode { get; set; }
}
