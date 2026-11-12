using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchCompaniesResponse
{
    [Display("Companies")]
    public List<CompanyResponse> Companies { get; set; } = [];

    [Display("Returned companies")]
    public int ReturnedCompanies { get; set; }

    [Display("Total matching records", Description = "Total reported by Apollo, including companies and saved accounts.")]
    public long? TotalMatchingRecords { get; set; }
}
