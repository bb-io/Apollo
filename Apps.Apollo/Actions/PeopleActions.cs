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
}
