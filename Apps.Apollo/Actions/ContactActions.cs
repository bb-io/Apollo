using Apps.Apollo.Constants;
using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Identifiers;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Actions;

[ActionList("Contacts")]
public class ContactActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Search contacts", Description = "Search your saved contacts by keywords or contact lists. Does not search the general people database.")]
    public async Task<SearchContactsResponse> SearchContacts([ActionParameter] SearchContactsRequest input)
    {
        var maximumResults = InputValidator.GetMaximumResults(input.MaximumResults);

        var result = await Client.Paginate<ContactsPageDto, ContactResponse>(
            (page, pageSize) => CreateSearchContactsRequest(input, page, pageSize),
            x => x.Contacts,
            maximumResults);

        return new()
        {
            Contacts = result.Items,
            ReturnedContacts = result.Items.Count,
            TotalMatchingContacts = result.TotalEntries
        };
    }
    
    [Action("Search contact lists", Description = "Search lists of saved contacts. Account and sequence lists are excluded.")]
    public async Task<SearchContactListsResponse> SearchContactLists([ActionParameter] SearchContactListsRequest input)
    {
        var lists = await Client.ExecuteWithErrorHandling<List<ContactListResponse>>(new RestRequest("labels"));

        return new()
        {
            Lists = lists
                .Where(x => x.Modality == ListModalities.Contacts)
                .Where(x => string.IsNullOrWhiteSpace(input.Name)
                            || x.Name.Contains(input.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList()
        };
    }

    [Action("Get contact", Description = "Get a saved contact with its profile, email, company, phone and list information.")]
    public async Task<ContactResponse> GetContact([ActionParameter] ContactIdentifier input)
    {
        if (string.IsNullOrWhiteSpace(input.ContactId))
            throw new PluginMisconfigurationException("Please fill in 'Contact ID'.");

        var request = new RestRequest($"contacts/{Uri.EscapeDataString(input.ContactId.Trim())}");
        var response = await Client.ExecuteWithErrorHandling<ContactWrapperDto>(request);

        return response.Contact;
    }

    private static RestRequest CreateSearchContactsRequest(SearchContactsRequest input, int page, int pageSize)
    {
        var body = new Dictionary<string, object>
            {
                ["sort_by_field"] = "contact_created_at",
                ["sort_ascending"] = true,
                ["page"] = page,
                ["per_page"] = pageSize
            }
            .AddIfNotEmpty("q_keywords", input.Keywords)
            .AddIfNotEmpty("contact_label_ids", input.ListIds);

        return new RestRequest("contacts/search", Method.Post).AddJsonBody(body);
    }
}
