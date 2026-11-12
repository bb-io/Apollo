using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Handlers.Static;

public class EmailBodyFormatDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() =>
    [
        new("plain", "Plain text"),
        new("html", "HTML")
    ];
}
