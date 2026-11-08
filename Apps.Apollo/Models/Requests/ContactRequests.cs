using Apps.Apollo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.Apollo.Models.Requests;
public class SearchContactsRequest
{
    [Display("Keywords", Description = "Search saved contacts by names, emails, job titles or company names.")] public string? Keywords { get; set; }
    [Display("List IDs", Description = "Include contacts belonging to any selected contact list."), DataSource(typeof(ContactListDataSourceHandler))] public string[]? ListIds { get; set; }
    [Display("Maximum results", Description = "Defaults to 100; maximum 50,000. Leave filters blank to search all saved contacts up to this limit.")] public int? MaximumResults { get; set; }
}
public class ContactIdentifier
{
    [Display("Contact ID", Description = "Saved Apollo contact ID, not an Apollo database person ID."), DataSource(typeof(ContactDataSourceHandler))]
    public string ContactId { get; set; } = "";
}
public class SearchContactListsRequest
{
    [Display("List name", Description = "Optional part of a list name. Only lists for contacts are returned.")] public string? Name { get; set; }
}
