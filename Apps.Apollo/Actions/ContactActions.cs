using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo.Actions;
[ActionList("Contacts")]
public class ContactActions(InvocationContext context) : Invocable(context)
{
    [Action("Search contacts", Description = "Search your saved contacts by keywords or contact lists, up to Maximum results. Does not search the general people database or enrich missing details.")]
    public Task<SearchContactsResponse> SearchContacts([ActionParameter] SearchContactsRequest input)
    {
        var (body, limit) = ContactMapping.Validate(input);
        return Client.SearchContactsAsync(body, limit);
    }
    [Action("Get contact", Description = "Retrieve a saved contact by Contact ID and output available profile, email, company, phone and list information.")]
    public async Task<ContactResponse> GetContact([ActionParameter] ContactIdentifier input)
    {
        if (string.IsNullOrWhiteSpace(input.ContactId)) throw new PluginMisconfigurationException("Select or enter a saved Apollo Contact ID, not a database Person ID.");
        var contact = await Client.GetContactAsync(input.ContactId);
        if (contact.ContactId != input.ContactId.Trim()) throw new PluginApplicationException("Apollo returned a different contact ID than requested. Check the ID before syncing the result.");
        return contact;
    }
    [Action("Search contact lists", Description = "Search lists for saved contacts, excluding account and sequence lists.")]
    public async Task<SearchContactListsResponse> SearchContactLists([ActionParameter] SearchContactListsRequest input)
        => new() { Lists = await Client.GetContactListsAsync(input.Name) };
}
