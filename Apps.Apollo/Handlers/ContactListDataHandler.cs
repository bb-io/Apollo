using Apps.Apollo.Constants;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Handlers;

public class ContactListDataHandler(InvocationContext invocationContext)
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var lists = await Client.ExecuteWithErrorHandling<List<ContactListResponse>>(new RestRequest("labels"));

        return lists
            .Where(x => x.Modality == ListModalities.Contacts)
            .Where(x => string.IsNullOrWhiteSpace(context.SearchString)
                        || x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.ListId, x.Name));
    }
}
