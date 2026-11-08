using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
namespace Apps.Apollo.Services;
internal static class CommunicationMapping
{
    internal static int Limit(int? value, int maximum)
    {
        var limit = value ?? 100;
        if (limit < 1 || limit > maximum) throw new PluginMisconfigurationException($"Set Maximum results between 1 and {maximum}.");
        return limit;
    }
    internal static List<KeyValuePair<string, string>> Calls(SearchCallsRequest input)
    {
        var filters = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(input.Keywords)) filters.Add(new("q_keywords", input.Keywords.Trim()));
        if (input.Direction != null)
        {
            if (input.Direction is not ("incoming" or "outgoing")) throw new PluginMisconfigurationException("Select Incoming or Outgoing for Direction.");
            filters.Add(new("inbound", input.Direction));
        }
        foreach (var id in EmailMapping.Clean(input.UserIds)) filters.Add(new("user_ids[]", id));
        foreach (var id in EmailMapping.Clean(input.PurposeIds)) filters.Add(new("phone_call_purpose_ids[]", id));
        foreach (var id in EmailMapping.Clean(input.OutcomeIds)) filters.Add(new("phone_call_outcome_ids[]", id));
        if (input.StartDate.HasValue || input.EndDate.HasValue)
        {
            if (!input.StartDate.HasValue || !input.EndDate.HasValue || input.EndDate.Value.Date <= input.StartDate.Value.Date)
                throw new PluginMisconfigurationException("Supply both Start date and End date, with End date later than Start date.");
            filters.Add(new("date_range[min]", input.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            filters.Add(new("date_range[max]", input.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }
        if (input.MinimumDuration.HasValue || input.MaximumDuration.HasValue)
        {
            if (!input.MinimumDuration.HasValue || !input.MaximumDuration.HasValue || input.MinimumDuration < 0 || input.MaximumDuration <= input.MinimumDuration)
                throw new PluginMisconfigurationException("Supply both duration bounds in seconds, with 0 <= Minimum duration < Maximum duration.");
            filters.Add(new("duration[min]", input.MinimumDuration.Value.ToString(CultureInfo.InvariantCulture)));
            filters.Add(new("duration[max]", input.MaximumDuration.Value.ToString(CultureInfo.InvariantCulture)));
        }
        return filters;
    }
    internal static Dictionary<string, object> Conversations(SearchConversationsRequest input)
    {
        var body = new Dictionary<string, object>();
        if (input.Type != null)
        {
            if (input.Type is not ("video_conference" or "phone_call")) throw new PluginMisconfigurationException("Select Video meeting or Phone call as Conversation type.");
            body["conversation_type"] = input.Type;
        }
        if (!string.IsNullOrWhiteSpace(input.AccountId)) body["account_id"] = input.AccountId.Trim();
        foreach (var (key, ids) in new[] { ("contact_ids", input.ContactIds), ("organization_ids", input.CompanyIds), ("tag_ids", input.TagIds) })
        { var cleaned = EmailMapping.Clean(ids); if (cleaned.Length > 0) body[key] = cleaned; }
        if (input.StartTime != null || input.EndTime != null)
        {
            var start = Time(input.StartTime); var end = Time(input.EndTime);
            if (end <= start) throw new PluginMisconfigurationException("End time must be later than Start time.");
            body["date_range"] = new { start = start.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture), end = end.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture) };
        }
        return body;
    }
    private static DateTimeOffset Time(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value.Trim(), @"T.*(Z|[+-]\d{2}:\d{2})$", RegexOptions.IgnoreCase)
            || !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            throw new PluginMisconfigurationException("Supply both conversation times as ISO 8601 timestamps with Z or an explicit UTC offset.");
        return time;
    }
    internal static (T[] Items, long? Total, int? TotalPages, bool Partial) Page<T>(string content, string key, int page, Func<T, string?> identity)
    {
        try
        {
            var payload = JObject.Parse(content);
            if (payload[key] is not JArray records) throw new PluginApplicationException($"Apollo omitted the {key} search results.");
            var items = records.ToObject<T[]>()!;
            if (items.Any(x => x == null || string.IsNullOrWhiteSpace(identity(x)))) throw new PluginApplicationException("Apollo returned a search record without its ID.");
            var meta = payload["pagination"] as JObject;
            if ((int?)meta?["page"] is int actual && actual != page) throw new PluginApplicationException("Apollo returned the wrong search page.");
            var total = (long?)meta?["total_entries"]; var pages = (int?)meta?["total_pages"];
            if (total < 0 || pages < 0) throw new PluginApplicationException("Apollo returned invalid pagination.");
            if (typeof(T) == typeof(ConversationResponse))
                foreach (var item in items.Cast<ConversationResponse>()) { item.AccountIds ??= []; item.ParticipantNames ??= []; }
            return (items, total, pages, (bool?)payload["partial_results_only"] == true);
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable communication search results."); }
        catch (FormatException) { throw new PluginApplicationException("Apollo returned unreadable communication pagination."); }
        catch (ArgumentException) { throw new PluginApplicationException("Apollo returned unreadable communication data."); }
    }
    internal static ConversationDetailsResponse Details(string content, string requestedId)
    {
        try
        {
            var payload = JObject.Parse(content);
            // The detail endpoint returns the conversation directly, without a wrapper.
            var mapped = (JObject)payload.DeepClone();
            mapped.Remove("participants"); // Source is grouped; the public output is a flattened participant collection.
            var dto = mapped.ToObject<ConversationDetailsResponse>()!;
            if (string.IsNullOrWhiteSpace(dto.ConversationId) || dto.ConversationId != requestedId.Split('_', 2)[0])
                throw new PluginApplicationException("Apollo returned a different or missing conversation ID. Check it before syncing the result.");
            dto.AccountIds ??= []; dto.ParticipantNames ??= []; dto.DealIds ??= []; dto.Transcript ??= [];
            if (dto.Transcript.Any(x => x == null)) throw new PluginApplicationException("Apollo returned an unreadable transcript segment.");
            dto.AudioUrl = (string?)payload["audio_recording"]?["url"];
            dto.VideoUrl = (string?)payload["video_recording"]?["url"];
            dto.TranscriptText = string.Join(Environment.NewLine, dto.Transcript.Where(x => !string.IsNullOrWhiteSpace(x.Sentence))
                .Select(x => string.IsNullOrWhiteSpace(x.ParticipantName) ? x.Sentence : $"{x.ParticipantName}: {x.Sentence}"));
            var participants = payload["participants"] as JObject;
            var people = new List<ConversationParticipantResponse>();
            if (participants?["internal"] is JArray internalPeople) people.AddRange(internalPeople.OfType<JObject>().Select(x => x.ToObject<ConversationParticipantResponse>()!));
            if (participants?["external"] is JObject external)
                foreach (var group in external.Properties().Where(x => x.Value is JArray))
                    people.AddRange(((JArray)group.Value).OfType<JObject>().Select(x => x.ToObject<ConversationParticipantResponse>()!));
            dto.Participants = people.ToArray();
            return dto;
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable conversation details."); }
        catch (ArgumentException) { throw new PluginApplicationException("Apollo returned unreadable conversation details."); }
    }
}
