using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Responses;

public class OutreachEmailResponse
{
    [Display("Email message ID")]
    [JsonProperty("id")]
    public string MessageId { get; set; } = string.Empty;

    [Display("Contact ID")]
    [JsonProperty("contact_id")]
    public string? ContactId { get; set; }

    [Display("Account ID")]
    [JsonProperty("account_id")]
    public string? AccountId { get; set; }

    [Display("Sequence ID")]
    [JsonProperty("emailer_campaign_id")]
    public string? SequenceId { get; set; }

    [Display("Sequence name")]
    [JsonProperty("campaign_name")]
    public string? SequenceName { get; set; }

    [Display("Sender user ID")]
    [JsonProperty("user_id")]
    public string? SenderUserId { get; set; }

    [Display("Email account ID")]
    [JsonProperty("email_account_id")]
    public string? EmailAccountId { get; set; }

    [Display("Provider message ID")]
    [JsonProperty("provider_message_id")]
    public string? ProviderMessageId { get; set; }

    [Display("Provider thread ID")]
    [JsonProperty("provider_thread_id")]
    public string? ProviderThreadId { get; set; }

    [Display("Subject")]
    [JsonProperty("subject")]
    public string? Subject { get; set; }

    [Display("Status")]
    [JsonProperty("status")]
    public string? Status { get; set; }

    [Display("Sender email")]
    [JsonProperty("from_email")]
    public string? FromEmail { get; set; }

    [Display("Sender name")]
    [JsonProperty("from_name")]
    public string? FromName { get; set; }

    [Display("Recipient email")]
    [JsonProperty("to_email")]
    public string? ToEmail { get; set; }

    [Display("Recipient name")]
    [JsonProperty("to_name")]
    public string? ToName { get; set; }

    [Display("Created at")]
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Display("Scheduled at")]
    [JsonProperty("due_at")]
    public DateTime? ScheduledAt { get; set; }

    [Display("Sent at")]
    [JsonProperty("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Display("Failed at")]
    [JsonProperty("failed_at")]
    public DateTime? FailedAt { get; set; }

    [Display("Failure reason")]
    [JsonProperty("failure_reason")]
    public string? FailureReason { get; set; }

    [Display("Not sent reason")]
    [JsonProperty("not_sent_reason")]
    public string? NotSentReason { get; set; }

    [Display("Reply classification")]
    [JsonProperty("reply_class")]
    public string? ReplyClass { get; set; }

    [Display("Plain text body")]
    [JsonProperty("body_text")]
    public string? BodyText { get; set; }

    [Display("HTML body")]
    [JsonProperty("body_html")]
    public string? BodyHtml { get; set; }

    [Display("Recipients")]
    [JsonProperty("recipients")]
    public List<OutreachEmailRecipientResponse> Recipients { get; set; } = [];
}
