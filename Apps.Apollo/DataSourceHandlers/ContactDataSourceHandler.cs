using Apps.Apollo.Services;
using Apps.Apollo.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo.DataSourceHandlers;
public class ContactDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var (body, limit) = ContactMapping.Validate(new SearchContactsRequest { Keywords = context.SearchString, MaximumResults = 30 });
        var result = await Client.SearchContactsAsync(body, limit);
        cancellationToken.ThrowIfCancellationRequested();
        return result.Contacts.Select(x => new DataSourceItem(x.ContactId!, $"{x.Name ?? x.Email ?? "Contact"} ({x.ContactId})"));
    }
}
public class ContactListDataSourceHandler(InvocationContext context) : Invocable(context), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var lists = await Client.GetContactListsAsync(context.SearchString);
        cancellationToken.ThrowIfCancellationRequested();
        return lists.Take(30).Select(x => new DataSourceItem(x.ListId, $"{x.Name} ({x.ListId})"));
    }
}
