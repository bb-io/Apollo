using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchContactListsResponse
{
    [Display("Contact lists")]
    public List<ContactListResponse> Lists { get; set; } = [];
}
