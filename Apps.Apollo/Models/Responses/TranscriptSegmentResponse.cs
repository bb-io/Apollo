using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class TranscriptSegmentResponse
{
    [Display("Transcript segment ID")]
    [JsonProperty("id")]
    public string? SegmentId { get; set; }

    [Display("Participant ID")]
    [JsonProperty("participant_id")]
    public string? ParticipantId { get; set; }

    [Display("Participant name")]
    [JsonProperty("participant_name")]
    public string? ParticipantName { get; set; }

    [Display("Spoken sentence")]
    [JsonProperty("spoken_sentence")]
    public string? Sentence { get; set; }
}
