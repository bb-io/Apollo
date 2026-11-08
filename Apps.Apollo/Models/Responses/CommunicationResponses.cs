using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
namespace Apps.Apollo.Models.Responses;
public class CallResponse
{
    [Display("Call ID")] [JsonProperty("id")] public string? CallId { get; set; }
    [Display("Contact ID")] [JsonProperty("contact_id")] public string? ContactId { get; set; }
    [Display("Account ID")] [JsonProperty("account_id")] public string? AccountId { get; set; }
    [Display("User ID")] [JsonProperty("user_id")] public string? UserId { get; set; }
    [Display("Conversation ID")] [JsonProperty("conversation_id")] public string? ConversationId { get; set; }
    [Display("From number")] [JsonProperty("from_number")] public string? FromNumber { get; set; }
    [Display("To number")] [JsonProperty("to_number")] public string? ToNumber { get; set; }
    [Display("Start time")] [JsonProperty("start_time")] public string? StartTime { get; set; }
    [Display("End time")] [JsonProperty("end_time")] public string? EndTime { get; set; }
    [Display("Status")] [JsonProperty("status")] public string? Status { get; set; }
    [Display("Note")] [JsonProperty("note_text")] public string? Note { get; set; }
    [Display("Recording URL")] [JsonProperty("recording_url")] public string? RecordingUrl { get; set; }
    [Display("Call purpose ID")] [JsonProperty("phone_call_purpose_id")] public string? PurposeId { get; set; }
    [Display("Call outcome ID")] [JsonProperty("phone_call_outcome_id")] public string? OutcomeId { get; set; }
    [Display("Incoming call")] [JsonProperty("inbound")] public bool? Incoming { get; set; }
    [Display("Duration", Description = "Seconds.")] [JsonProperty("duration")] public double? Duration { get; set; }
    [Display("Transcribed")] [JsonProperty("transcribed")] public bool? Transcribed { get; set; }
}
public class ConversationResponse
{
    [Display("Conversation ID")] [JsonProperty("id")] public string? ConversationId { get; set; }
    [Display("Topic")] [JsonProperty("topic")] public string? Topic { get; set; }
    [Display("Start time")] [JsonProperty("start_time")] public string? StartTime { get; set; }
    [Display("Duration", Description = "Seconds.")] [JsonProperty("duration")] public int? Duration { get; set; }
    [Display("Processing state")] [JsonProperty("state")] public string? State { get; set; }
    [Display("Host")] [JsonProperty("host")] public string? Host { get; set; }
    [Display("Host user ID")] [JsonProperty("host_id")] public string? HostId { get; set; }
    [Display("Conversation type")] [JsonProperty("conversation_type")] public string? Type { get; set; }
    [Display("Private")] [JsonProperty("is_private")] public bool? Private { get; set; }
    [Display("Can access conversation")] [JsonProperty("can_access_conversation")] public bool? CanAccess { get; set; }
    [Display("Account IDs")] [JsonProperty("account_ids")] public string[] AccountIds { get; set; } = [];
    [Display("Participant names")] [JsonProperty("participant_names")] public string[] ParticipantNames { get; set; } = [];
}
public class ConversationDetailsResponse : ConversationResponse
{
    [Display("Audio recording URL")] public string? AudioUrl { get; set; }
    [Display("Video recording URL")] public string? VideoUrl { get; set; }
    [Display("Transcript")] [JsonProperty("transcript")] public TranscriptSegmentResponse[] Transcript { get; set; } = [];
    [Display("Transcript text", Description = "Sentences in Apollo's returned order, with participant names when available.")] public string TranscriptText { get; set; } = "";
    [Display("Participants")] public ConversationParticipantResponse[] Participants { get; set; } = [];
    [Display("Deal IDs")] [JsonProperty("opportunity_ids")] public string[] DealIds { get; set; } = [];
    [Display("Failure code")] [JsonProperty("failure_code")] public string? FailureCode { get; set; }
}
public class TranscriptSegmentResponse
{
    [Display("Transcript segment ID")] [JsonProperty("id")] public string? SegmentId { get; set; }
    [Display("Participant ID")] [JsonProperty("participant_id")] public string? ParticipantId { get; set; }
    [Display("Participant name")] [JsonProperty("participant_name")] public string? ParticipantName { get; set; }
    [Display("Spoken sentence")] [JsonProperty("spoken_sentence")] public string? Sentence { get; set; }
}
public class ConversationParticipantResponse
{
    [Display("Participant ID")] [JsonProperty("id")] public string? ParticipantId { get; set; }
    [Display("Name")] [JsonProperty("name")] public string? Name { get; set; }
    [Display("Email")] [JsonProperty("email")] public string? Email { get; set; }
    [Display("Contact ID")] [JsonProperty("contact_id")] public string? ContactId { get; set; }
    [Display("Account ID")] [JsonProperty("account_id")] public string? AccountId { get; set; }
    [Display("Company ID")] [JsonProperty("organization_id")] public string? CompanyId { get; set; }
}
public class SearchCallsResponse
{
    [Display("Calls")] public CallResponse[] Calls { get; set; } = [];
    [Display("Returned calls")] public int ReturnedCalls { get; set; }
    [Display("Total matching calls")] public long? TotalMatchingCalls { get; set; }
    [Display("Pages retrieved")] public int PagesRetrieved { get; set; }
    [Display("Results limited")] public bool ResultsLimited { get; set; }
}
public class SearchConversationsResponse
{
    [Display("Conversations")] public ConversationResponse[] Conversations { get; set; } = [];
    [Display("Returned conversations")] public int ReturnedConversations { get; set; }
    [Display("Total matching conversations")] public long? TotalMatchingConversations { get; set; }
    [Display("Pages retrieved")] public int PagesRetrieved { get; set; }
    [Display("Results limited")] public bool ResultsLimited { get; set; }
}
