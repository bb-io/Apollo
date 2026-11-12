using Apps.Apollo.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class HandlerTests : TestBase
{
    [TestMethod]
    public async Task ContactDataHandler_ReturnsContacts()
    {
        var handler = new ContactDataHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        PrintDataHandlerResult(result);
        Assert.IsTrue(result.Any());
    }

    [TestMethod]
    public async Task ContactListDataHandler_ReturnsContactLists()
    {
        var handler = new ContactListDataHandler(InvocationContext);

        var result = await handler.GetDataAsync(new DataSourceContext(), CancellationToken.None);

        PrintDataHandlerResult(result);
        Assert.IsTrue(result.Any());
    }
}
