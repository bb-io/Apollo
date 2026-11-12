using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Handlers.Static;

public class CallDirectionDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("incoming", "Incoming"),
        new("outgoing", "Outgoing")
    ];
}
