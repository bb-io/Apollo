using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class PeopleActionsTests : TestBase
{
    [TestMethod]
    public async Task EnrichPerson_ByEmailOfSavedContact_ReturnsPerson()
    {
        var contacts = await new ContactActions(InvocationContext).SearchContacts(new SearchContactsRequest { MaximumResults = 20 });
        var email = contacts.Contacts.First(x => !string.IsNullOrWhiteSpace(x.Email)).Email;
        var actions = new PeopleActions(InvocationContext);

        var result = await actions.EnrichPerson(new EnrichPersonRequest { Email = email });

        PrintJsonResult(result);
        Assert.IsTrue(result.MatchFound);
    }

    [TestMethod]
    public async Task EnrichPerson_WithoutIdentity_ThrowsMisconfiguration()
    {
        var actions = new PeopleActions(InvocationContext);

        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(
            () => actions.EnrichPerson(new EnrichPersonRequest { Domain = "apollo.io" }));
    }
}
