using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;
using System.Globalization;
namespace Apps.Apollo.Api;
public partial class ApolloClient
{
    internal async Task<SearchCallsResponse> SearchCallsAsync(List<KeyValuePair<string, string>> filters, int limit)
    {
        var data = await CommunicationPages<CallResponse>("phone_calls/search", "phone_calls", limit, 100, c => c.CallId,
            (page, size) => { var request = new RestRequest("phone_calls/search", Method.Get); foreach (var (key, value) in filters) request.AddQueryParameter(key, value);
                return request.AddQueryParameter("page", page.ToString(CultureInfo.InvariantCulture)).AddQueryParameter("per_page", size.ToString(CultureInfo.InvariantCulture)); });
        return new() { Calls = data.Items, ReturnedCalls = data.Items.Length, TotalMatchingCalls = data.Total, PagesRetrieved = data.Pages, ResultsLimited = data.Limited };
    }
    internal async Task<SearchConversationsResponse> SearchConversationsAsync(Dictionary<string, object> filters, int limit)
    {
        // 25 is a conservative app page size matching Apollo's examples, not an asserted API maximum.
        var data = await CommunicationPages<ConversationResponse>("conversations/search", "conversations", limit, 25, c => c.ConversationId,
            (page, size) => new RestRequest("conversations/search", Method.Post).AddJsonBody(new Dictionary<string, object>(filters) { ["page"] = page, ["num_fetch_result"] = size }));
        return new() { Conversations = data.Items, ReturnedConversations = data.Items.Length, TotalMatchingConversations = data.Total, PagesRetrieved = data.Pages, ResultsLimited = data.Limited };
    }
    private async Task<(T[] Items, long? Total, int Pages, bool Limited)> CommunicationPages<T>(string resource, string key, int limit, int defaultSize, Func<T, string?> identity, Func<int, int, RestRequest> request)
    {
        var results = new List<T>(); var seen = new HashSet<string>(StringComparer.Ordinal);
        var size = Math.Min(defaultSize, limit); var budget = Math.Min(500, (limit + size - 1) / size);
        long? total = null; bool limited = false; int fetched = 0;
        for (var page = 1; page <= budget; page++)
        {
            RestResponse response;
            try { response = await ExecuteWithErrorHandling(request(page, size)); }
            catch (PluginMisconfigurationException ex) { throw new PluginMisconfigurationException($"Search page {page} failed for {resource}. {ex.Message}"); }
            var data = CommunicationMapping.Page(response.Content ?? "", key, page, identity);
            fetched++; total = data.Total ?? total; limited |= data.Partial;
            int added = 0;
            foreach (var item in data.Items) { if (results.Count >= limit) break; if (seen.Add(identity(item)!)) { results.Add(item); added++; } }
            bool more = data.TotalPages.HasValue ? page < data.TotalPages.Value : data.Items.Length >= size;
            if (results.Count >= limit || page == budget) { limited |= more || (total.HasValue && total > results.Count); break; }
            if (!more) break;
            if (data.Items.Length > 0 && added == 0) throw new PluginApplicationException("Apollo repeated a communication search page. Stopped to avoid further requests.");
        }
        return (results.ToArray(), total, fetched, limited);
    }
    internal async Task<ConversationDetailsResponse> GetConversationAsync(string id)
    {
        var response = await ExecuteWithErrorHandling(new RestRequest("conversations/" + Uri.EscapeDataString(id), Method.Get));
        return CommunicationMapping.Details(response.Content ?? "", id);
    }
}
