using Apps.Apollo.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Models.Requests;

public class SearchContactsRequest
{
    [Display("Keywords", Description = "Search saved contacts by names, emails, job titles or company names.")]
    public string? Keywords { get; set; }

    [Display("List IDs", Description = "Include contacts belonging to any selected contact list.")]
    [DataSource(typeof(ContactListDataHandler))]
    public IEnumerable<string>? ListIds { get; set; }

    [Display("Maximum results", Description = "Defaults to 100, maximum 50,000.")]
    public int? MaximumResults { get; set; }
}
