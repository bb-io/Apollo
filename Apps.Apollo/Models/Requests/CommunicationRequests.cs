using Apps.Apollo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.Apollo.Models.Requests;
public class SearchCallsRequest
{
    [Display("Keywords")] public string? Keywords { get; set; }
    [Display("Direction"), StaticDataSource(typeof(CallDirectionDataSourceHandler))] public string? Direction { get; set; }
    [Display("User IDs")] public string[]? UserIds { get; set; }
    [Display("Call purpose IDs", Description = "Find purpose IDs in Apollo's call filter URL.")] public string[]? PurposeIds { get; set; }
    [Display("Call outcome IDs", Description = "Find outcome IDs in Apollo's disposition filter URL.")] public string[]? OutcomeIds { get; set; }
    [Display("Start date", Description = "Supply both calendar dates; End date must be later.")] public DateTime? StartDate { get; set; }
    [Display("End date")] public DateTime? EndDate { get; set; }
    [Display("Minimum duration", Description = "Seconds. Supply both duration bounds.")] public int? MinimumDuration { get; set; }
    [Display("Maximum duration", Description = "Seconds. Must exceed Minimum duration.")] public int? MaximumDuration { get; set; }
    [Display("Maximum results", Description = "Defaults to 100; maximum 50,000.")] public int? MaximumResults { get; set; }
}
public class SearchConversationsRequest
{
    [Display("Conversation type"), StaticDataSource(typeof(ConversationTypeDataSourceHandler))] public string? Type { get; set; }
    [Display("Account ID")] public string? AccountId { get; set; }
    [Display("Contact IDs"), DataSource(typeof(ContactDataSourceHandler))] public string[]? ContactIds { get; set; }
    [Display("Company IDs", Description = "Apollo database organization IDs, not saved account IDs.")] public string[]? CompanyIds { get; set; }
    [Display("Tag IDs")] public string[]? TagIds { get; set; }
    [Display("Start time", Description = "ISO 8601 with an explicit timezone, for example 2026-11-01T00:00:00Z. Supply both times.")] public string? StartTime { get; set; }
    [Display("End time", Description = "ISO 8601 with an explicit timezone. Must be later than Start time.")] public string? EndTime { get; set; }
    [Display("Maximum results", Description = "Defaults to 100; app limit 10,000. Search retrieves summaries without requesting full details.")] public int? MaximumResults { get; set; }
}
public class ConversationIdentifier
{
    [Display("Conversation ID", Description = "Use an ID from Search conversations. Apollo also supports id_shareid. Detail retrieval can consume one credit if AI insights exist.")]
    public string ConversationId { get; set; } = "";
}
