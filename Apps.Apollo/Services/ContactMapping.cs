using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Apps.Apollo.Services;
internal static class ContactMapping
{
    internal static (Dictionary<string, object> Body, int Limit) Validate(SearchContactsRequest input)
    {
        var limit = input.MaximumResults ?? 100;
        if (limit < 1 || limit > 50000) throw new PluginMisconfigurationException("Set Maximum results between 1 and 50,000.");
        var body = new Dictionary<string, object> { ["sort_by_field"] = "contact_created_at", ["sort_ascending"] = true };
        if (!string.IsNullOrWhiteSpace(input.Keywords)) body["q_keywords"] = input.Keywords.Trim();
        var ids = (input.ListIds ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length > 0) body["contact_label_ids"] = ids;
        return (body, limit);
    }
    internal static ContactResponse Normalize(ContactResponse contact)
    {
        if (string.IsNullOrWhiteSpace(contact.ContactId)) throw new PluginApplicationException("Apollo returned a contact without a contact ID.");
        contact.ListIds ??= [];
        contact.PhoneNumbers ??= [];
        return contact;
    }
    internal static ContactResponse Single(string content)
    {
        try
        {
            var payload = JObject.Parse(content);
            var contact = payload["contact"] is JObject value ? value.ToObject<ContactResponse>() : null;
            if (contact == null) throw new PluginApplicationException("Apollo did not return the requested contact. Check the contact ID and retry.");
            return Normalize(contact);
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable contact data."); }
    }
    internal static ContactPage Search(string content, int requestedPage)
    {
        try
        {
            var payload = JObject.Parse(content);
            if (payload["contacts"] is not JArray contacts) throw new PluginApplicationException("Apollo omitted the contact search results.");
            var values = contacts.ToObject<ContactResponse?[]>()!;
            if (values.Any(x => x == null)) throw new PluginApplicationException("Apollo returned an empty entry in contact search results.");
            var pagination = payload["pagination"] as JObject;
            var page = (int?)pagination?["page"];
            if (page.HasValue && page != requestedPage) throw new PluginApplicationException("Apollo returned the wrong contact search page. Stopped to avoid repeated results.");
            var total = (long?)pagination?["total_entries"];
            var pages = (int?)pagination?["total_pages"];
            if (total < 0 || pages < 0) throw new PluginApplicationException("Apollo returned invalid contact pagination.");
            return new(values.Select(x => Normalize(x!)).ToArray(), total, pages, (bool?)payload["partial_results_only"] == true);
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable contact search results."); }
        catch (FormatException) { throw new PluginApplicationException("Apollo returned unreadable contact pagination."); }
        catch (ArgumentException) { throw new PluginApplicationException("Apollo returned unreadable contact search data."); }
    }
    internal static ContactListResponse[] Lists(string content, string? search = null)
    {
        try
        {
            var payload = JToken.Parse(content);
            if (payload is not JArray labels) throw new PluginApplicationException("Apollo returned an unexpected contact-list response.");
            var result = new List<ContactListResponse>();
            foreach (var token in labels)
            {
                if (token is not JObject label) throw new PluginApplicationException("Apollo returned an unreadable list entry.");
                if (!string.Equals((string?)label["modality"], "contacts", StringComparison.OrdinalIgnoreCase)) continue;
                var item = label.ToObject<ContactListResponse>()!;
                if (string.IsNullOrWhiteSpace(item.ListId)) item.ListId = (string?)label["_id"] ?? "";
                if (string.IsNullOrWhiteSpace(item.ListId) || string.IsNullOrWhiteSpace(item.Name))
                    throw new PluginApplicationException("Apollo returned a contact list without its name or ID.");
                if (string.IsNullOrWhiteSpace(search) || item.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)) result.Add(item);
            }
            return result.DistinctBy(x => x.ListId).ToArray();
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable contact lists."); }
    }
}
internal record ContactPage(ContactResponse[] Contacts, long? Total, int? TotalPages, bool Partial);
