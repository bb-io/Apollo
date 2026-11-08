using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
namespace Apps.Apollo.Models.Responses;
public class OutreachEmailResponse
{
    [Display("Email message ID")] [JsonProperty("id")] public string? MessageId { get; set; }
    [Display("Contact ID")] [JsonProperty("contact_id")] public string? ContactId { get; set; }
    [Display("Account ID")] [JsonProperty("account_id")] public string? AccountId { get; set; }
    [Display("Sequence ID")] [JsonProperty("emailer_campaign_id")] public string? SequenceId { get; set; }
    [Display("Sender user ID")] [JsonProperty("user_id")] public string? SenderUserId { get; set; }
    [Display("Email account ID")] [JsonProperty("email_account_id")] public string? EmailAccountId { get; set; }
    [Display("Provider message ID")] [JsonProperty("provider_message_id")] public string? ProviderMessageId { get; set; }
    [Display("Provider thread ID")] [JsonProperty("provider_thread_id")] public string? ProviderThreadId { get; set; }
    [Display("Subject")] [JsonProperty("subject")] public string? Subject { get; set; }
    [Display("Status")] [JsonProperty("status")] public string? Status { get; set; }
    [Display("Sender email")] [JsonProperty("from_email")] public string? FromEmail { get; set; }
    [Display("Sender name")] [JsonProperty("from_name")] public string? FromName { get; set; }
    [Display("Recipient email")] [JsonProperty("to_email")] public string? ToEmail { get; set; }
    [Display("Recipient name")] [JsonProperty("to_name")] public string? ToName { get; set; }
    [Display("Sequence name")] [JsonProperty("campaign_name")] public string? SequenceName { get; set; }
    [Display("Created at")] [JsonProperty("created_at")] public string? CreatedAt { get; set; }
    [Display("Scheduled at")] [JsonProperty("due_at")] public string? ScheduledAt { get; set; }
    [Display("Sent at")] [JsonProperty("completed_at")] public string? CompletedAt { get; set; }
    [Display("Failed at")] [JsonProperty("failed_at")] public string? FailedAt { get; set; }
    [Display("Failure reason")] [JsonProperty("failure_reason")] public string? FailureReason { get; set; }
    [Display("Not sent reason")] [JsonProperty("not_sent_reason")] public string? NotSentReason { get; set; }
    [Display("Reply classification")] [JsonProperty("reply_class")] public string? ReplyClass { get; set; }
    [Display("Plain text body")] [JsonProperty("body_text")] public string? BodyText { get; set; }
    [Display("HTML body")] [JsonProperty("body_html")] public string? BodyHtml { get; set; }
    [Display("HTML body loaded")] [JsonProperty("body_html_loaded")] public bool? HtmlBodyLoaded { get; set; }
    [Display("Recipients")] [JsonProperty("recipients")] public EmailRecipientResponse[] Recipients { get; set; } = [];
}
public class EmailRecipientResponse
{
    [Display("Email address")] [JsonProperty("email")] public string? Email { get; set; }
    [Display("Recipient type")] [JsonProperty("type")] public string? Type { get; set; }
}
public class SearchOutreachEmailsResponse
{
    [Display("Emails")] public OutreachEmailResponse[] Emails { get; set; } = [];
    [Display("Returned emails")] public int ReturnedEmails { get; set; }
    [Display("Total matching emails")] public long? TotalMatchingEmails { get; set; }
    [Display("Pages retrieved")] public int PagesRetrieved { get; set; }
    [Display("Results limited")] public bool ResultsLimited { get; set; }
}
public class EmailContentResponse
{
    [Display("Email message ID")] [JsonProperty("id")] public string? MessageId { get; set; }
    [Display("Subject")] [JsonProperty("subject")] public string? Subject { get; set; }
    [Display("Body")] [JsonProperty("body")] public string? Body { get; set; }
    [Display("Body format")] [JsonProperty("body_format")] public string? BodyFormat { get; set; }
    [Display("Status")] [JsonProperty("status")] public string? Status { get; set; }
    [Display("Sent at")] [JsonProperty("sent_at")] public string? SentAt { get; set; }
    [Display("Recipients")] [JsonProperty("recipients")] public EmailRecipientResponse[] Recipients { get; set; } = [];
}
public class EmailContentResult
{
    [Display("Input position")] public int InputPosition { get; set; }
    [Display("Email message ID")] public string MessageId { get; set; } = "";
    [Display("Content available", Description = "False when Apollo omits the ID; this does not identify why the email was unavailable.")] public bool ContentAvailable { get; set; }
    [Display("Email content")] public EmailContentResponse? Email { get; set; }
}
public class GetEmailContentResponse
{
    [Display("Results")] public EmailContentResult[] Results { get; set; } = [];
    [Display("Requested emails")] public int RequestedEmails { get; set; }
    [Display("Available emails")] public int AvailableEmails { get; set; }
    [Display("Unavailable emails")] public int UnavailableEmails { get; set; }
}
