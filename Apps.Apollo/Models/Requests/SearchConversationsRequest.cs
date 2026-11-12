using Apps.Apollo.Handlers;
using Apps.Apollo.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Models.Requests;

public class SearchConversationsRequest
{
    [Display("Conversation type")]
    [StaticDataSource(typeof(ConversationTypeDataHandler))]
    public string? Type { get; set; }

    [Display("Account ID")]
    public string? AccountId { get; set; }

    [Display("Contact IDs")]
    [DataSource(typeof(ContactDataHandler))]
    public IEnumerable<string>? ContactIds { get; set; }

    [Display("Company IDs", Description = "Apollo database organization IDs, not saved account IDs.")]
    public IEnumerable<string>? CompanyIds { get; set; }

    [Display("Tag IDs")]
    public IEnumerable<string>? TagIds { get; set; }

    [Display("Start time")]
    public DateTime? StartTime { get; set; }

    [Display("End time")]
    public DateTime? EndTime { get; set; }

    [Display("Maximum results", Description = "Defaults to 100, maximum 10,000.")]
    public int? MaximumResults { get; set; }
}
