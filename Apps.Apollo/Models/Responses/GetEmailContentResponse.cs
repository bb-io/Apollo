using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class GetEmailContentResponse
{
    [Display("Results")]
    public List<EmailContentResult> Results { get; set; } = [];

    [Display("Requested emails")]
    public int RequestedEmails { get; set; }

    [Display("Available emails")]
    public int AvailableEmails { get; set; }

    [Display("Unavailable emails")]
    public int UnavailableEmails { get; set; }
}
