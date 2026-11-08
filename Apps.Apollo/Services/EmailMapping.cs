using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
namespace Apps.Apollo.Services;
internal static class EmailMapping
{
    private static readonly HashSet<string> Statuses = ["delivered", "scheduled", "drafted", "not_opened", "opened", "clicked", "unsubscribed", "demoed", "bounced", "spam_blocked", "failed_other"];
    internal static string[] Clean(string[]? values) => (values ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.Ordinal).ToArray();
    internal static (List<KeyValuePair<string, string>> Filters, int Limit) Validate(SearchOutreachEmailsRequest input)
    {
        var limit = input.MaximumResults ?? 100;
        if (limit < 1 || limit > 50000) throw new PluginMisconfigurationException("Set Maximum results between 1 and 50,000.");
        var filters = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(input.Keywords)) filters.Add(new("q_keywords", input.Keywords.Trim()));
        foreach (var id in Clean(input.SequenceIds)) filters.Add(new("emailer_campaign_ids[]", id));
        foreach (var id in Clean(input.UserIds)) filters.Add(new("user_ids[]", id));
        foreach (var status in Clean(input.Statuses))
        {
            if (!Statuses.Contains(status)) throw new PluginMisconfigurationException("Select a supported Email status. Use the delivered filter for sent emails; completed is a response status, not a search filter.");
            filters.Add(new("emailer_message_stats[]", status));
        }
        var basis = input.DateBasis ?? "completed_at";
        if (basis is not ("completed_at" or "due_at")) throw new PluginMisconfigurationException("Select Sent date or Scheduled date for Date basis.");
        if (input.StartDate.HasValue || input.EndDate.HasValue || input.DateBasis != null)
        {
            if (!input.StartDate.HasValue || !input.EndDate.HasValue || input.EndDate.Value.Date <= input.StartDate.Value.Date)
                throw new PluginMisconfigurationException("Supply both Start date and End date, with End date later than Start date.");
            filters.Add(new("emailer_message_date_range_mode", basis));
            filters.Add(new("emailer_message_date_range[min]", input.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            filters.Add(new("emailer_message_date_range[max]", input.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }
        return (filters, limit);
    }
    internal static (string[] Ids, string Format) ValidateContent(GetEmailContentRequest input)
    {
        if (input.EmailMessageIds == null || input.EmailMessageIds.Length == 0 || input.EmailMessageIds.Any(string.IsNullOrWhiteSpace))
            throw new PluginMisconfigurationException("Supply at least one Email message ID, with no empty entries.");
        var format = input.BodyFormat ?? "plain";
        if (format is not ("plain" or "html")) throw new PluginMisconfigurationException("Select Plain text or HTML as Body format.");
        return (input.EmailMessageIds.Select(x => x.Trim()).ToArray(), format);
    }
    internal static JObject Object(string content)
    {
        try { return JObject.Parse(content); }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable email data."); }
    }
    internal static EmailSearchPage Search(string content, int requestedPage)
    {
        try
        {
            var payload = Object(content);
            if (payload["emailer_messages"] is not JArray messages) throw new PluginApplicationException("Apollo omitted the email search results.");
            var result = new List<OutreachEmailResponse>();
            foreach (var token in messages)
            {
                if (token is not JObject original) throw new PluginApplicationException("Apollo returned an unreadable email search entry.");
                var message = (JObject)original.DeepClone();
                if (message["recipients"] is JArray recipients)
                    foreach (var item in recipients.OfType<JObject>()) item["type"] ??= item["recipient_type_cd"]?.DeepClone();
                var dto = message.ToObject<OutreachEmailResponse>()!;
                if (string.IsNullOrWhiteSpace(dto.MessageId)) throw new PluginApplicationException("Apollo returned an email without an email message ID.");
                dto.Recipients ??= [];
                result.Add(dto);
            }
            var pagination = payload["pagination"] as JObject;
            var page = (int?)pagination?["page"];
            if (page.HasValue && page != requestedPage) throw new PluginApplicationException("Apollo returned the wrong email search page.");
            var total = (long?)pagination?["total_entries"];
            var pages = (int?)pagination?["total_pages"];
            if (total < 0 || pages < 0) throw new PluginApplicationException("Apollo returned invalid email pagination.");
            return new(result.ToArray(), total, pages, (bool?)payload["partial_results_only"] == true);
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable email search results."); }
        catch (FormatException) { throw new PluginApplicationException("Apollo returned unreadable email pagination."); }
        catch (ArgumentException) { throw new PluginApplicationException("Apollo returned unreadable email search data."); }
    }
    internal static Dictionary<string, EmailContentResponse> Content(string content, string[] requested, string format)
    {
        try
        {
            var payload = Object(content);
            if (payload["emailer_messages"] is not JArray messages) throw new PluginApplicationException("Apollo omitted the email content results.");
            var allowed = requested.ToHashSet(StringComparer.Ordinal);
            var result = new Dictionary<string, EmailContentResponse>(StringComparer.Ordinal);
            foreach (var token in messages)
            {
                var dto = token is JObject entry ? entry.ToObject<EmailContentResponse>() : null;
                if (dto == null || string.IsNullOrWhiteSpace(dto.MessageId) || !allowed.Contains(dto.MessageId))
                    throw new PluginApplicationException("Apollo returned content for an unexpected email ID. Stopped to avoid incorrect CRM associations.");
                if (dto.BodyFormat != format || dto.Status != "completed")
                    throw new PluginApplicationException("Apollo returned unexpected email format or send status.");
                dto.Recipients ??= [];
                if (!result.TryAdd(dto.MessageId, dto)) throw new PluginApplicationException("Apollo returned duplicate email content entries.");
            }
            return result;
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable email content."); }
    }
    internal static GetEmailContentResponse Restore(string[] ids, Dictionary<string, EmailContentResponse> content)
    {
        var rows = ids.Select((id, index) => new EmailContentResult { InputPosition = index + 1, MessageId = id, ContentAvailable = content.ContainsKey(id), Email = content.GetValueOrDefault(id) }).ToArray();
        var found = rows.Count(x => x.ContentAvailable);
        return new() { Results = rows, RequestedEmails = rows.Length, AvailableEmails = found, UnavailableEmails = rows.Length - found };
    }
}
internal record EmailSearchPage(OutreachEmailResponse[] Emails, long? Total, int? TotalPages, bool Partial);
