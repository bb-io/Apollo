using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchOutreachEmailsResponse
{
    [Display("Emails")]
    public List<OutreachEmailResponse> Emails { get; set; } = [];

    [Display("Returned emails")]
    public int ReturnedEmails { get; set; }
}
