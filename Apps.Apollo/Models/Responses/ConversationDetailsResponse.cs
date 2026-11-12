using Apps.Apollo.Models.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class ConversationDetailsResponse : ConversationResponse
{
    [Display("Audio recording URL")]
    public string? AudioUrl { get; set; }

    [Display("Video recording URL")]
    public string? VideoUrl { get; set; }

    [Display("Transcript")]
    public List<TranscriptSegmentResponse> Transcript { get; set; } = [];

    [Display("Transcript text", Description = "Transcript sentences in order, prefixed with participant names when available.")]
    public string TranscriptText { get; set; } = string.Empty;

    [Display("Participants")]
    public List<ConversationParticipantResponse> Participants { get; set; } = [];

    [Display("Deal IDs")]
    public List<string> DealIds { get; set; } = [];

    [Display("Failure code")]
    public string? FailureCode { get; set; }

    public ConversationDetailsResponse()
    {
    }

    public ConversationDetailsResponse(ConversationDetailsDto dto)
    {
        ConversationId = dto.ConversationId;
        Topic = dto.Topic;
        StartTime = dto.StartTime;
        Duration = dto.Duration;
        State = dto.State;
        Host = dto.Host;
        HostId = dto.HostId;
        Type = dto.Type;
        Private = dto.Private;
        CanAccess = dto.CanAccess;
        AccountIds = dto.AccountIds ?? [];
        ParticipantNames = dto.ParticipantNames ?? [];
        AudioUrl = dto.AudioRecording?.Url;
        VideoUrl = dto.VideoRecording?.Url;
        Transcript = dto.Transcript ?? [];
        TranscriptText = BuildTranscriptText(Transcript);
        Participants = dto.Participants?.GetAll() ?? [];
        DealIds = dto.OpportunityIds ?? [];
        FailureCode = dto.FailureCode;
    }

    private static string BuildTranscriptText(IEnumerable<TranscriptSegmentResponse> transcript)
    {
        var lines = transcript
            .Where(x => !string.IsNullOrWhiteSpace(x.Sentence))
            .Select(x => string.IsNullOrWhiteSpace(x.ParticipantName) ? x.Sentence : $"{x.ParticipantName}: {x.Sentence}");

        return string.Join(Environment.NewLine, lines);
    }
}
