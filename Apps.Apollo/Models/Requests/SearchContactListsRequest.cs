using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Requests;

public class SearchContactListsRequest
{
    [Display("List name", Description = "Part of a list name. Only lists for contacts are returned.")]
    public string? Name { get; set; }
}
