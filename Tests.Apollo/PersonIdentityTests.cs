using Apps.Apollo.Models.Requests;
using Apps.Apollo.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class PersonIdentityTests
{
    [TestMethod]
    public void PersonIdentity_DomainsAreNormalized()
    {
        var body = PeopleEnrichmentMapping.ToDetails(new PersonIdentityInput { Name = " Person ", Domain = "https://www.example.com/team" });
        Assert.AreEqual("example.com", body["domain"]);
        Assert.AreEqual("Person", body["name"]);
        Assert.AreEqual(2, body.Count);
    }
}
