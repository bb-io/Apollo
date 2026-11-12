using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.Apollo;

public class Application : IApplication, ICategoryProvider
{
    public string Name => "Apollo";

    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.Crm];
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}
