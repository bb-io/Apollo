using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class CallResponse
{
    [Display("Call ID")]
    [JsonProperty("id")]
    public string CallId { get; set; } = string.Empty;

    [Display("Contact ID")]
    [JsonProperty("contact_id")]
    public string? ContactId { get; set; }

    [Display("Account ID")]
    [JsonProperty("account_id")]
    public string? AccountId { get; set; }

    [Display("User ID")]
    [JsonProperty("user_id")]
    public string? UserId { get; set; }

    [Display("Conversation ID")]
    [JsonProperty("conversation_id")]
    public string? ConversationId { get; set; }

    [Display("From number")]
    [JsonProperty("from_number")]
    public string? FromNumber { get; set; }

    [Display("To number")]
    [JsonProperty("to_number")]
    public string? ToNumber { get; set; }

    [Display("Start time")]
    [JsonProperty("start_time")]
    public DateTime? StartTime { get; set; }

    [Display("End time")]
    [JsonProperty("end_time")]
    public DateTime? EndTime { get; set; }

    [Display("Status")]
    [JsonProperty("status")]
    public string? Status { get; set; }

    [Display("Note")]
    [JsonProperty("note_text")]
    public string? Note { get; set; }

    [Display("Recording URL")]
    [JsonProperty("recording_url")]
    public string? RecordingUrl { get; set; }

    [Display("Call purpose ID")]
    [JsonProperty("phone_call_purpose_id")]
    public string? PurposeId { get; set; }

    [Display("Call outcome ID")]
    [JsonProperty("phone_call_outcome_id")]
    public string? OutcomeId { get; set; }

    [Display("Incoming call")]
    [JsonProperty("inbound")]
    public bool? Incoming { get; set; }

    [Display("Duration", Description = "Call duration in seconds.")]
    [JsonProperty("duration")]
    public double? Duration { get; set; }

    [Display("Transcribed")]
    [JsonProperty("transcribed")]
    public bool? Transcribed { get; set; }
}
