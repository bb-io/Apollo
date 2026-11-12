using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class EnrichPersonResponse
{
    [Display("Match found", Description = "Check the match confidence before updating an external record.")]
    public bool MatchFound { get; set; }

    [Display("Person")]
    public PersonResponse? Person { get; set; }
}
