using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;
using System.Globalization;
namespace Apps.Apollo.Api;
public partial class ApolloClient
{
    internal async Task<SearchOutreachEmailsResponse> SearchEmailsAsync(List<KeyValuePair<string, string>> filters, int limit)
    {
        var results = new List<OutreachEmailResponse>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        int size = Math.Min(100, limit), fetched = 0;
        int budget = Math.Min(500, (limit + size - 1) / size);
        long? total = null;
        bool limited = false;
        for (var page = 1; page <= budget; page++)
        {
            var request = new RestRequest("emailer_messages/search", Method.Get);
            foreach (var (key, value) in filters) request.AddQueryParameter(key, value);
            request.AddQueryParameter("page", page.ToString(CultureInfo.InvariantCulture));
            request.AddQueryParameter("per_page", size.ToString(CultureInfo.InvariantCulture));
            var response = await ExecuteWithErrorHandling(request);
            var data = EmailMapping.Search(response.Content ?? "", page);
            fetched++; total = data.Total ?? total; limited |= data.Partial;
            int added = 0;
            foreach (var email in data.Emails)
            {
                if (results.Count >= limit) break;
                if (seen.Add(email.MessageId!)) { results.Add(email); added++; }
            }
            bool more = data.TotalPages.HasValue ? page < data.TotalPages.Value : data.Emails.Length >= size;
            if (results.Count >= limit || page == budget) { limited |= more || (total.HasValue && total > results.Count); break; }
            if (!more) break;
            if (data.Emails.Length > 0 && added == 0) throw new PluginApplicationException("Apollo repeated an outreach email page. Stopped to avoid more requests.");
        }
        return new() { Emails = results.ToArray(), ReturnedEmails = results.Count, TotalMatchingEmails = total, PagesRetrieved = fetched, ResultsLimited = limited };
    }
    internal async Task<Dictionary<string, EmailContentResponse>> GetEmailContentAsync(string[] ids, string format)
    {
        var response = await ExecuteWithErrorHandling(new RestRequest("emailer_messages/get_content", Method.Post).AddJsonBody(new { ids, body_format = format }));
        return EmailMapping.Content(response.Content ?? "", ids, format);
    }
}
