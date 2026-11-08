using Blackbird.Applications.Sdk.Common;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Services;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
namespace Apps.Apollo.Actions;
[ActionList("People")]
public class PeopleActions(InvocationContext context) : Invocable(context)
{
    [Action("Enrich person", Description = "Match a person and output available email, company and profile information. Supply identifying details; enrichment may consume credits.")]
    public async Task<EnrichPersonResponse> EnrichPerson([ActionParameter] EnrichPersonRequest input)
    {
        var values = PeopleEnrichmentMapping.ToDetails(input);
        var request = new RestRequest("people/match", Method.Post);
        foreach (var (key, value) in values)
            if (!string.IsNullOrWhiteSpace(value)) request.AddQueryParameter(key, value.Trim());
        request.AddQueryParameter("reveal_personal_emails", input.RevealPersonalEmails == true ? "true" : "false");
        request.AddQueryParameter("reveal_phone_number", "false");
        var response = await Client.ExecuteWithErrorHandling(request);
        try
        {
            var payload = JObject.Parse(response.Content ?? "");
            if (payload["error_details"] is JObject) throw Client.CreateException(response);
            var person = payload["person"]?.Type == JTokenType.Null ? null : payload["person"]?.ToObject<PersonResponse>();
            var found = !string.IsNullOrWhiteSpace(person?.PersonId) && person.MatchConfidence != "none";
            if (person != null) person.PersonalEmails ??= [];
            return new() { MatchFound = found, Person = found ? person : null };
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned an unreadable person response. Retry the action."); }
    }
    [Action("Enrich people", Description = "Enrich multiple people in batches of ten, preserving input positions and source record IDs. May consume credits.")]
    public async Task<EnrichPeopleResponse> EnrichPeople([ActionParameter] EnrichPeopleRequest input)
    {
        if (input.People == null || input.People.Length == 0)
            throw new PluginMisconfigurationException("Add at least one person to the People collection.");

        // Validate and materialize every row before spending credits on the first batch.
        var details = new Dictionary<string, string>[input.People.Length];
        for (var index = 0; index < input.People.Length; index++)
        {
            try { details[index] = PeopleEnrichmentMapping.ToDetails(input.People[index]); }
            catch (PluginMisconfigurationException ex)
            { throw new PluginMisconfigurationException($"Person at input position {index + 1}: {ex.Message}"); }
        }

        var results = new List<PersonEnrichmentResult>(input.People.Length);
        decimal? credits = 0m;
        for (var offset = 0; offset < details.Length; offset += 10)
        {
            var count = Math.Min(10, details.Length - offset);
            try
            {
                var batch = await Client.EnrichPeopleAsync(details.Skip(offset).Take(count).ToArray(), input.RevealPersonalEmails == true);
                results.AddRange(PeopleEnrichmentMapping.MapBatch(batch, input.People.Skip(offset).Take(count).ToArray(), offset));
                credits = credits.HasValue && batch.CreditsConsumed.HasValue ? credits + batch.CreditsConsumed : null;
            }
            catch (PluginMisconfigurationException ex)
            {
                throw new PluginMisconfigurationException($"Batch for input positions {offset + 1}–{offset + count} failed. {offset} earlier inputs completed; their requests may have consumed credits. Fix the cause and retry only the failed and later inputs. {ex.Message}");
            }
            catch (PluginApplicationException ex)
            {
                throw new PluginApplicationException($"Batch for input positions {offset + 1}–{offset + count} failed. {offset} earlier inputs completed; those and the failing request may have consumed credits. Review before rerunning the collection. {ex.Message}");
            }
        }
        var matched = results.Count(x => x.MatchFound);
        return new() { Results = results.ToArray(), RequestedPeople = details.Length, MatchedPeople = matched,
            UnmatchedPeople = details.Length - matched, CreditsConsumed = credits };
    }
}
