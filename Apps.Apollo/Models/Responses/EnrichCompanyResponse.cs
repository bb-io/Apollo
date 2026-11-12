using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class EnrichCompanyResponse
{
    [Display("Match found")]
    public bool MatchFound { get; set; }

    [Display("Company")]
    public CompanyResponse? Company { get; set; }
}
