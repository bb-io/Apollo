using Apps.Apollo.Actions;
using Apps.Apollo.Models.Identifiers;
using Apps.Apollo.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class ContactActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchContacts_WithoutFilters_ReturnsContacts()
    {
        var actions = new ContactActions(InvocationContext);

        var result = await actions.SearchContacts(new SearchContactsRequest { MaximumResults = 5 });

        PrintJsonResult(result);
        Assert.AreEqual(5, result.ReturnedContacts);
    }

    [TestMethod]
    public async Task SearchContacts_AcrossSeveralPages_ReturnsUniqueContacts()
    {
        var actions = new ContactActions(InvocationContext);

        var result = await actions.SearchContacts(new SearchContactsRequest { MaximumResults = 150 });

        Console.WriteLine($"Returned: {result.ReturnedContacts}, total: {result.TotalMatchingContacts}");
        Assert.AreEqual(150, result.ReturnedContacts);
        Assert.AreEqual(150, result.Contacts.Select(x => x.ContactId).Distinct().Count());
    }

    [TestMethod]
    public async Task SearchContacts_ByList_ReturnsListContacts()
    {
        var actions = new ContactActions(InvocationContext);
        var lists = await actions.SearchContactLists(new SearchContactListsRequest());
        var list = lists.Lists.First(x => x.CachedCount > 0);

        var result = await actions.SearchContacts(new SearchContactsRequest { ListIds = [list.ListId], MaximumResults = 5 });

        PrintJsonResult(result);
        Assert.IsTrue(result.Contacts.All(x => x.ListIds.Contains(list.ListId)));
    }

    [TestMethod]
    public async Task GetContact_ExistingContact_ReturnsContact()
    {
        var actions = new ContactActions(InvocationContext);
        var contacts = await actions.SearchContacts(new SearchContactsRequest { MaximumResults = 1 });
        var contactId = contacts.Contacts.First().ContactId;

        var result = await actions.GetContact(new ContactIdentifier { ContactId = contactId });

        PrintJsonResult(result);
        Assert.AreEqual(contactId, result.ContactId);
    }

    [TestMethod]
    public async Task GetContact_UnknownContact_ThrowsMisconfiguration()
    {
        var actions = new ContactActions(InvocationContext);

        var exception = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(
            () => actions.GetContact(new ContactIdentifier { ContactId = "000000000000000000000000" }));

        Console.WriteLine(exception.Message);
    }

    [TestMethod]
    public async Task SearchContactLists_WithoutName_ReturnsContactLists()
    {
        var actions = new ContactActions(InvocationContext);

        var result = await actions.SearchContactLists(new SearchContactListsRequest());

        PrintJsonResult(result);
        Assert.IsTrue(result.Lists.Count > 0);
    }
}
