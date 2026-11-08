using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class OfflineTests
{
    private static PeopleActions Actions() => new(new InvocationContext
    { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", "offline-unused-key") } });
    [TestMethod]
    public async Task MissingIdentity_IsRejectedBeforeNetworkCall()
    {
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichPerson(new EnrichPersonRequest { Domain = "apollo.io" }));
    }
    [TestMethod]
    public async Task InvalidDomain_IsRejectedBeforeNetworkCall()
    {
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichPerson(new EnrichPersonRequest { Name = "Test Person", Domain = "person@company.com" }));
    }
    [TestMethod]
    public void PersonMapping_PreservesConfidenceAndDistinctIds()
    {
        var person = JsonConvert.DeserializeObject<PersonResponse>("""
        {"id":"person-1","contact_id":"contact-2","email_status":"verified","match_confidence":"low","organization":{"id":"company-3","primary_domain":"example.com","estimated_num_employees":42}}
        """);
        Assert.IsNotNull(person);
        Assert.AreEqual("person-1", person.PersonId);
        Assert.AreEqual("contact-2", person.ContactId);
        Assert.AreEqual("low", person.MatchConfidence);
        Assert.AreEqual("company-3", person.Company?.CompanyId);
        Assert.AreEqual(42, person.Company?.EstimatedEmployees);
        Assert.IsNotNull(person.PersonalEmails);
    }
}
