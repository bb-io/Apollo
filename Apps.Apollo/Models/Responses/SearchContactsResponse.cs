using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchContactsResponse
{
    [Display("Contacts")]
    public List<ContactResponse> Contacts { get; set; } = [];

    [Display("Returned contacts")]
    public int ReturnedContacts { get; set; }

    [Display("Total matching contacts")]
    public long? TotalMatchingContacts { get; set; }
}
