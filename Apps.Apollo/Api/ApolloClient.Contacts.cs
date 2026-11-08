using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Exceptions;
using RestSharp;
namespace Apps.Apollo.Api;
public partial class ApolloClient
{
    internal async Task<ContactResponse> GetContactAsync(string contactId)
    {
        var response = await ExecuteWithErrorHandling(new RestRequest("contacts/" + Uri.EscapeDataString(contactId.Trim()), Method.Get));
        return ContactMapping.Single(response.Content ?? "");
    }
    internal async Task<ContactListResponse[]> GetContactListsAsync(string? name)
    {
        var response = await ExecuteWithErrorHandling(new RestRequest("labels", Method.Get));
        return ContactMapping.Lists(response.Content ?? "", name);
    }
    internal async Task<SearchContactsResponse> SearchContactsAsync(Dictionary<string, object> filters, int limit)
    {
        var results = new List<ContactResponse>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        int pageSize = Math.Min(100, limit), budget = Math.Min(500, (limit + Math.Min(100, limit) - 1) / Math.Min(100, limit)), fetched = 0;
        long? total = null;
        bool limited = false;
        for (var page = 1; page <= budget; page++)
        {
            var body = new Dictionary<string, object>(filters) { ["page"] = page, ["per_page"] = pageSize };
            ContactPage data;
            try
            {
                var response = await ExecuteWithErrorHandling(new RestRequest("contacts/search", Method.Post).AddJsonBody(body));
                data = ContactMapping.Search(response.Content ?? "", page);
            }
            catch (PluginMisconfigurationException ex) { throw new PluginMisconfigurationException($"Contact search page {page} failed. {ex.Message}"); }
            catch (PluginApplicationException ex) { throw new PluginApplicationException($"Contact search page {page} failed. {ex.Message}"); }
            fetched++;
            total = data.Total ?? total;
            limited |= data.Partial;
            int added = 0;
            foreach (var contact in data.Contacts)
            {
                if (results.Count >= limit) break;
                if (seen.Add(contact.ContactId!)) { results.Add(contact); added++; }
            }
            var more = data.TotalPages.HasValue ? page < data.TotalPages : data.Contacts.Length >= pageSize;
            if (results.Count >= limit || page == budget)
            { limited |= more || (total.HasValue && total > results.Count); break; }
            if (!more) break;
            if (data.Contacts.Length > 0 && added == 0)
                throw new PluginApplicationException("Apollo repeated a contact page. Stopped to avoid more requests; retry the search later.");
        }
        return new() { Contacts = results.ToArray(), ReturnedContacts = results.Count, TotalMatchingContacts = total, PagesRetrieved = fetched, ResultsLimited = limited };
    }
}
