using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Handlers.Static;

public class EmailDateBasisDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("completed_at", "Sent date"),
        new("due_at", "Scheduled date")
    ];
}
