using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Actions;

[ActionList("People")]
public class PeopleActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const string NoMatchConfidence = "none";

    [Action("Enrich person", Description = "Match a person and output their email, company and profile information. Can consume credits.")]
    public async Task<EnrichPersonResponse> EnrichPerson([ActionParameter] EnrichPersonRequest input)
    {
        ValidatePersonIdentity(input);

        var request = new RestRequest("people/match", Method.Post)
            .AddOptionalQueryParameter("id", input.PersonId)
            .AddOptionalQueryParameter("email", input.Email)
            .AddOptionalQueryParameter("linkedin_url", input.LinkedinUrl)
            .AddOptionalQueryParameter("name", input.Name)
            .AddOptionalQueryParameter("first_name", input.FirstName)
            .AddOptionalQueryParameter("last_name", input.LastName)
            .AddOptionalQueryParameter("domain", input.Domain?.ToBareDomain())
            .AddOptionalQueryParameter("organization_name", input.OrganizationName)
            .AddQueryParameter("reveal_personal_emails", input.RevealPersonalEmails ?? false);

        var response = await Client.ExecuteWithErrorHandling<PersonMatchDto>(request);
        var matchFound = !string.IsNullOrWhiteSpace(response.Person?.PersonId)
                         && response.Person.MatchConfidence != NoMatchConfidence;

        return new()
        {
            MatchFound = matchFound,
            Person = matchFound ? response.Person : null
        };
    }

    private static void ValidatePersonIdentity(EnrichPersonRequest input)
    {
        var hasIdentifier = new[] { input.PersonId, input.Email, input.LinkedinUrl, input.Name }
            .Any(x => !string.IsNullOrWhiteSpace(x));
        var hasFirstAndLastName = !string.IsNullOrWhiteSpace(input.FirstName) && !string.IsNullOrWhiteSpace(input.LastName);

        if (!hasIdentifier && !hasFirstAndLastName)
            throw new PluginMisconfigurationException("Please fill in 'Person ID', 'Email address', 'LinkedIn URL', 'Full name', or both 'First name' and 'Last name'.");
    }
}
