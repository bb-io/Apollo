using Apps.Apollo.Actions;
using Apps.Apollo.DataSourceHandlers;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class ContactTests
{
    [TestMethod]
    public void SearchBody_UsesDocumentedFiltersAndDefaultLimit()
    {
        var (body, limit) = ContactMapping.Validate(new() { Keywords = " Test ", ListIds = ["list1", " list1 ", "", "list2"] });
        Assert.AreEqual(100, limit);
        Assert.AreEqual("Test", body["q_keywords"]);
        CollectionAssert.AreEqual(new[] { "list1", "list2" }, (string[])body["contact_label_ids"]);
        Assert.AreEqual("contact_created_at", body["sort_by_field"]);
        Assert.AreEqual(true, body["sort_ascending"]);
    }
    [TestMethod]
    public void BlankFiltersAllowBoundedExportButInvalidLimitsAreRejected()
    {
        var (body, limit) = ContactMapping.Validate(new());
        Assert.AreEqual(100, limit);
        Assert.IsFalse(body.ContainsKey("q_keywords"));
        Assert.ThrowsException<PluginMisconfigurationException>(() => ContactMapping.Validate(new() { MaximumResults = 0 }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => ContactMapping.Validate(new() { MaximumResults = 50001 }));
    }
    [TestMethod]
    public void ContactMapping_KeepsAllIdentifierTypesDistinct()
    {
        var contact = ContactMapping.Single("""
        {"contact":{"id":"contact1","person_id":"person1","organization_id":"company1","account_id":"account1","label_ids":null,"phone_numbers":null,"email_status":"verified","email_unsubscribed":true}}
        """);
        Assert.AreEqual("contact1", contact.ContactId);
        Assert.AreEqual("person1", contact.PersonId);
        Assert.AreEqual("company1", contact.CompanyId);
        Assert.AreEqual("account1", contact.AccountId);
        Assert.AreEqual(0, contact.ListIds.Length);
        Assert.AreEqual(0, contact.PhoneNumbers.Length);
        Assert.AreEqual(true, contact.EmailUnsubscribed);
    }
    [TestMethod]
    public void Lists_FilterContactModalityAndSearchByName()
    {
        var lists = ContactMapping.Lists("""
        [{"id":"list1","name":"Demo Leads","modality":"contacts","cached_count":3},{"id":"other","name":"Demo Accounts","modality":"accounts"},{"_id":"list2","name":"Other Leads","modality":"contacts"}]
        """, "DEMO");
        Assert.AreEqual(1, lists.Length);
        Assert.AreEqual("list1", lists[0].ListId);
        Assert.AreEqual(3, lists[0].CachedCount);
        Assert.AreEqual("list2", ContactMapping.Lists("""[{"_id":"list2","name":"Other","modality":"contacts"}]""")[0].ListId);
    }
    [TestMethod]
    public void EmptySearchAndLists_ReturnEmptyCollections()
    {
        Assert.AreEqual(0, ContactMapping.Search("""{"contacts":[],"pagination":{"page":1,"total_entries":0,"total_pages":0}}""", 1).Contacts.Length);
        Assert.AreEqual(0, ContactMapping.Lists("[]").Length);
    }
    [TestMethod]
    public void MalformedContactsAndWrongPage_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => ContactMapping.Single("{}"));
        Assert.ThrowsException<PluginApplicationException>(() => ContactMapping.Single("""{"contact":{"person_id":"p1"}}"""));
        Assert.ThrowsException<PluginApplicationException>(() => ContactMapping.Search("""{"contacts":[],"pagination":{"page":1}}""", 2));
        Assert.ThrowsException<PluginApplicationException>(() => ContactMapping.Search("""{"contacts":[null]}""", 1));
    }
    [TestMethod]
    public async Task BlankContactId_IsRejectedBeforeNetworkCall()
    {
        var actions = new ContactActions(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", "offline-unused-key") } });
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => actions.GetContact(new()));
    }
}
[TestClass]
[TestCategory("Integration")]
public class ContactIntegrationTests
{
    private static InvocationContext Context()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Configure your local Apollo API key.");
        using var config = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? config.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Configure your local Apollo API key.");
        return new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", key!) } };
    }
    [TestMethod]
    public async Task SearchContacts_ReturnsSavedContacts()
    {
        var result = await new ContactActions(Context()).SearchContacts(new() { MaximumResults = 5 });
        if (result.ReturnedContacts == 0) Assert.Inconclusive("This workspace has no saved contacts available to the key.");
        Assert.IsTrue(result.ReturnedContacts <= 5);
        Assert.AreEqual(result.ReturnedContacts, result.Contacts.Length);
        Assert.AreEqual(1, result.PagesRetrieved);
        Assert.IsTrue(result.Contacts.All(c => !string.IsNullOrWhiteSpace(c.ContactId)));
        Assert.AreEqual(result.ReturnedContacts, result.Contacts.Select(c => c.ContactId).Distinct().Count());
    }
    [TestMethod]
    public async Task GetContact_ReturnsSameSavedContact()
    {
        var actions = new ContactActions(Context());
        var sample = await actions.SearchContacts(new() { MaximumResults = 1 });
        if (sample.ReturnedContacts == 0) Assert.Inconclusive("Save at least one contact in Apollo before running this test.");
        var result = await actions.GetContact(new() { ContactId = sample.Contacts[0].ContactId! });
        Assert.AreEqual(sample.Contacts[0].ContactId, result.ContactId);
        Assert.IsNotNull(result.ListIds);
        Assert.IsNotNull(result.PhoneNumbers);
    }
    [TestMethod]
    public async Task SearchContactLists_ReturnsContactLists()
    {
        var result = await new ContactActions(Context()).SearchContactLists(new());
        Assert.IsNotNull(result.Lists);
        Assert.IsTrue(result.Lists.All(x => x.Modality == "contacts" && !string.IsNullOrWhiteSpace(x.ListId)));
        Assert.AreEqual(result.Lists.Length, result.Lists.Select(x => x.ListId).Distinct().Count());
    }
    [TestMethod]
    public async Task SearchContacts_ByListReturnsMembers()
    {
        var actions = new ContactActions(Context());
        var sample = await actions.SearchContacts(new() { MaximumResults = 100 });
        var listId = sample.Contacts.SelectMany(c => c.ListIds).FirstOrDefault();
        if (listId == null) Assert.Inconclusive("None of the first 100 saved contacts belongs to a list. Add a contact to a list to test filtering.");
        var result = await actions.SearchContacts(new() { ListIds = [listId!], MaximumResults = 5 });
        Assert.IsTrue(result.ReturnedContacts > 0, "Selected list may have changed during the test.");
        Assert.IsTrue(result.Contacts.All(c => c.ListIds.Contains(listId!)), "Apollo returned a contact outside the selected list.");
    }
    [TestMethod]
    public async Task ContactDropdowns_ReturnUniqueItems()
    {
        var context = Context();
        var contacts = (await new ContactDataSourceHandler(context).GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None)).ToArray();
        var lists = (await new ContactListDataSourceHandler(context).GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None)).ToArray();
        Assert.IsTrue(contacts.Length <= 30);
        Assert.IsTrue(lists.Length <= 30);
    }
    [TestMethod]
    public async Task SearchContacts_PaginatesWhenEnoughSavedContactsExist()
    {
        var result = await new ContactActions(Context()).SearchContacts(new() { MaximumResults = 101 });
        if (result.TotalMatchingContacts < 101) Assert.Inconclusive("At least 101 saved contacts are required to verify the two-page boundary.");
        Assert.AreEqual(2, result.PagesRetrieved);
        Assert.AreEqual(101, result.ReturnedContacts);
        Assert.AreEqual(101, result.Contacts.Select(c => c.ContactId).Distinct().Count());
    }
}
