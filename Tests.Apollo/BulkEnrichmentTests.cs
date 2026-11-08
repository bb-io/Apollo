using Apps.Apollo.Actions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class BulkEnrichmentTests
{
    private static PeopleActions Actions() => new(new InvocationContext
    { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", "offline-unused-key") } });
    [TestMethod]
    public async Task EmptyCollection_IsRejectedBeforeNetworkCall()
        => await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichPeople(new()));
    [TestMethod]
    public async Task InvalidLaterRow_IsRejectedBeforeFirstNetworkCall()
    {
        var exception = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => Actions().EnrichPeople(new()
        {
            People = Enumerable.Range(0, 11).Select(i => i == 10 ? new PersonEnrichmentInput() : new PersonEnrichmentInput { PersonId = "known" }).ToArray()
        }));
        StringAssert.Contains(exception.Message, "position 11");
    }
    [TestMethod]
    public void NullSlotsAndDuplicateMatches_PreserveSourcePositions()
    {
        var batch = JsonConvert.DeserializeObject<BulkPeopleDto>("""
        {"status":"success","total_requested_enrichments":3,"credits_consumed":0.5,"matches":[{"id":"p1","match_confidence":"high"},null,{"id":"p1","personal_emails":null}]}
        """)!;
        var inputs = new[] { new PersonEnrichmentInput { PersonId = "p1", SourceRecordId = "A" }, new PersonEnrichmentInput { SourceRecordId = "B" }, new PersonEnrichmentInput { PersonId = "p1", SourceRecordId = "C" } };
        var results = PeopleEnrichmentMapping.MapBatch(batch, inputs, 10);
        CollectionAssert.AreEqual(new[] { 11, 12, 13 }, results.Select(x => x.InputPosition).ToArray());
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, results.Select(x => x.SourceRecordId).ToArray());
        Assert.IsTrue(results[0].MatchFound);
        Assert.IsFalse(results[1].MatchFound);
        Assert.IsNull(results[1].Person);
        Assert.IsTrue(results[2].MatchFound);
        Assert.IsNotNull(results[2].Person!.PersonalEmails);
        Assert.AreEqual(0.5m, batch.CreditsConsumed);
    }
    [TestMethod]
    public void MissingSlots_AreRejectedRatherThanShiftingMatches()
    {
        Assert.ThrowsException<PluginApplicationException>(() => PeopleEnrichmentMapping.MapBatch(
            new() { Matches = [new PersonResponse { PersonId = "p1" }] }, [new(), new()], 0));
    }
    [TestMethod]
    public void ReorderedKnownIds_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => PeopleEnrichmentMapping.MapBatch(
            new() { Matches = [new PersonResponse { PersonId = "p2" }, new PersonResponse { PersonId = "p1" }] },
            [new() { PersonId = "p1" }, new() { PersonId = "p2" }], 0));
    }
    [TestMethod]
    public void NoConfidenceMatch_IsKeptButNoneIsNotMatched()
    {
        var rows = PeopleEnrichmentMapping.MapBatch(new() { Matches = [new() { PersonId = "p1" }, new() { PersonId = "p2", MatchConfidence = "none" }] }, [new(), new()], 0);
        Assert.IsTrue(rows[0].MatchFound);
        Assert.IsNull(rows[0].Person!.MatchConfidence);
        Assert.IsFalse(rows[1].MatchFound);
        Assert.IsNull(rows[1].Person);
    }
    [TestMethod]
    public void SourceIds_AreLocalAndDomainsAreNormalized()
    {
        var body = PeopleEnrichmentMapping.ToDetails(new PersonEnrichmentInput { Name = " Person ", Domain = "https://www.example.com/team", SourceRecordId = "crm-1" });
        Assert.AreEqual("example.com", body["domain"]);
        Assert.AreEqual("Person", body["name"]);
        Assert.IsFalse(body.ContainsKey("SourceRecordId"));
        Assert.AreEqual(2, body.Count);
    }
}
