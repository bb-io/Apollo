using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class EmailContentResult
{
    [Display("Input position")]
    public int InputPosition { get; set; }

    [Display("Email message ID")]
    public string MessageId { get; set; } = string.Empty;

    [Display("Content available", Description = "False when Apollo returned no sent email for this ID.")]
    public bool ContentAvailable { get; set; }

    [Display("Email content")]
    public EmailContentResponse? Email { get; set; }
}
