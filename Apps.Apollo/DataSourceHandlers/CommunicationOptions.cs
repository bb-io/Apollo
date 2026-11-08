using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.Apollo.DataSourceHandlers;
public class CallDirectionDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => [new("incoming", "Incoming"), new("outgoing", "Outgoing")];
}
public class ConversationTypeDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => [new("video_conference", "Video meeting"), new("phone_call", "Phone call")];
}
