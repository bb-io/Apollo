using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Handlers;

public class ContactDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    private const int PageSize = 50;

    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object> { ["per_page"] = PageSize }
            .AddIfNotEmpty("q_keywords", context.SearchString);

        var request = new RestRequest("contacts/search", Method.Post).AddJsonBody(body);
        var response = await Client.ExecuteWithErrorHandling<ContactsPageDto>(request);

        return response.Contacts.Select(x => new DataSourceItem(x.ContactId, x.Name ?? x.Email ?? x.ContactId));
    }
}
