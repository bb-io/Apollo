using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
namespace Apps.Apollo.DataSourceHandlers;
public class EmailStatusDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => [new("delivered", "Delivered"), new("scheduled", "Scheduled"), new("drafted", "Draft"),
        new("not_opened", "Not opened"), new("opened", "Opened"), new("clicked", "Clicked"), new("unsubscribed", "Unsubscribed"),
        new("demoed", "Demo booked"), new("bounced", "Bounced"), new("spam_blocked", "Blocked as spam"), new("failed_other", "Other failure")];
}
public class EmailDateBasisDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => [new("completed_at", "Sent date"), new("due_at", "Scheduled date")];
}
public class EmailBodyFormatDataSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => [new("plain", "Plain text"), new("html", "HTML")];
}
