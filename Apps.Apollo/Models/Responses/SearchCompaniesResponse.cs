using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Responses;
public class SearchCompaniesResponse
{
    [Display("Companies")] public CompanyResponse[] Companies { get; set; } = [];
    [Display("Returned companies")] public int ReturnedCompanies { get; set; }
    [Display("Total matching records", Description = "Total reported by Apollo, when available. Includes companies and saved accounts returned by Apollo.")] public long? TotalMatchingRecords { get; set; }
    [Display("Pages retrieved")] public int PagesRetrieved { get; set; }
    [Display("Results limited", Description = "More results may exist because of the result limit, page budget, Apollo access limits or duplicate records.")] public bool ResultsLimited { get; set; }
    [Display("Saved accounts included", Description = "True if Apollo returned saved accounts. Their account IDs are kept separate from company IDs.")] public bool SavedAccountsIncluded { get; set; }
}
