using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Globalization;
namespace Apps.Apollo.Api;
public partial class ApolloClient
{
    internal async Task<SearchCompaniesResponse> SearchCompaniesAsync(List<KeyValuePair<string, string>> filters, int limit)
    {
        var results = new List<CompanyResponse>();
        var seen = new Dictionary<string, CompanyResponse>(StringComparer.Ordinal);
        var pageSize = Math.Min(100, limit);
        // Bound paid page requests even if duplicates reduce the returned company count.
        var pageBudget = Math.Min(500, (limit + pageSize - 1) / pageSize);
        int pages = 0;
        long? total = null;
        bool limited = false, accounts = false;
        for (var page = 1; page <= pageBudget; page++)
        {
            var request = new RestRequest("mixed_companies/search", Method.Post);
            foreach (var (key, value) in filters) request.AddQueryParameter(key, value);
            request.AddQueryParameter("page", page.ToString(CultureInfo.InvariantCulture));
            request.AddQueryParameter("per_page", pageSize.ToString(CultureInfo.InvariantCulture));
            RestResponse response;
            try { response = await ExecuteWithErrorHandling(request); }
            catch (PluginMisconfigurationException ex) { throw new PluginMisconfigurationException($"Company search page {page} failed after {pages} pages completed. Earlier pages may have consumed credits. {ex.Message}"); }
            catch (PluginApplicationException ex) { throw new PluginApplicationException($"Company search page {page} failed after {pages} pages completed. Earlier/current requests may have consumed credits. {ex.Message}"); }
            CompanySearchPage data;
            try
            {
                var payload = JObject.Parse(response.Content ?? "");
                if (payload["error_details"] is JObject) throw CreateException(response);
                data = CompanySearch.Parse(response.Content ?? "", page);
            }
            catch (Newtonsoft.Json.JsonException) { throw new PluginApplicationException($"Apollo returned unreadable search page {page}. Earlier/current requests may have consumed credits."); }
            pages++;
            total = data.TotalEntries ?? total;
            accounts |= data.AccountCount > 0;
            limited |= data.Partial;
            var newResults = 0;
            foreach (var company in data.Companies)
            {
                if (CompanySearch.AddCompany(results, seen, company, limit)) newResults++;
            }
            bool morePages = data.TotalPages.HasValue ? page < data.TotalPages.Value : data.Companies.Length >= pageSize;
            if (results.Count >= limit || page == pageBudget || page == 500)
            {
                limited |= morePages || (total.HasValue && total > results.Count);
                break;
            }
            if (!morePages) break;
            if (data.Companies.Length > 0 && newResults == 0)
                throw new PluginApplicationException($"Apollo search page {page} repeated earlier companies. Stopped to avoid further credit usage; review before retrying.");
        }
        return new() { Companies = results.ToArray(), ReturnedCompanies = results.Count, TotalMatchingRecords = total,
            PagesRetrieved = pages, ResultsLimited = limited, SavedAccountsIncluded = accounts };
    }
}
