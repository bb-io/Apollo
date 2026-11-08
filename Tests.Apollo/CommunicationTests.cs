using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Text.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class CommunicationTests
{
    [TestMethod]
    public void CallFilters_UseDocumentedDirectionAndSeconds()
    {
        var fields = CommunicationMapping.Calls(new() { Direction = "incoming", Keywords = " test ", MinimumDuration = 0, MaximumDuration = 60,
            UserIds = ["u1"], PurposeIds = ["p1"], OutcomeIds = ["o1"], StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 2) });
        Assert.AreEqual("incoming", fields.Single(x => x.Key == "inbound").Value);
        Assert.AreEqual("60", fields.Single(x => x.Key == "duration[max]").Value);
        Assert.AreEqual("2026-01-01", fields.Single(x => x.Key == "date_range[min]").Value);
        Assert.AreEqual("p1", fields.Single(x => x.Key == "phone_call_purpose_ids[]").Value);
        Assert.AreEqual("o1", fields.Single(x => x.Key == "phone_call_outcome_ids[]").Value);
    }
    [TestMethod]
    public void InvalidCallRangesAndDirections_AreRejected()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Calls(new() { Direction = "true" }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Calls(new() { MinimumDuration = 60, MaximumDuration = 30 }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Calls(new() { MinimumDuration = 1 }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Calls(new() { StartDate = DateTime.Today }));
    }
    [TestMethod]
    public void ConversationFilters_KeepContactsAccountsAndCompaniesDistinct()
    {
        var body = CommunicationMapping.Conversations(new() { AccountId = "a1", ContactIds = [" c1 ", "c1"], CompanyIds = ["org1"], TagIds = ["tag1"], Type = "phone_call" });
        Assert.AreEqual("a1", body["account_id"]);
        CollectionAssert.AreEqual(new[] { "c1" }, (string[])body["contact_ids"]);
        CollectionAssert.AreEqual(new[] { "org1" }, (string[])body["organization_ids"]);
        Assert.AreEqual("phone_call", body["conversation_type"]);
    }
    [TestMethod]
    public void ConversationTimes_AreConvertedFromExplicitOffsetToUtc()
    {
        var body = CommunicationMapping.Conversations(new() { StartTime = "2026-11-01T08:00:00-05:00", EndTime = "2026-11-01T09:00:00-05:00" });
        var range = JObject.FromObject(body["date_range"]);
        Assert.AreEqual("2026-11-01T13:00:00.0000000Z", (string?)range["start"]);
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Conversations(new() { StartTime = "2026-11-01T08:00:00", EndTime = "2026-11-01T09:00:00" }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => CommunicationMapping.Conversations(new() { Type = "email" }));
    }
    [TestMethod]
    public void CallResponse_KeepsConversationAndContactIdsDistinct()
    {
        var result = CommunicationMapping.Page<CallResponse>("""
        {"phone_calls":[{"id":"call1","contact_id":"contact1","account_id":"account1","conversation_id":"conversation1","inbound":true,"duration":30}],"pagination":{"page":"1","total_entries":1,"total_pages":1}}
        """, "phone_calls", 1, x => x.CallId);
        Assert.AreEqual("call1", result.Items[0].CallId);
        Assert.AreEqual("contact1", result.Items[0].ContactId);
        Assert.AreEqual("conversation1", result.Items[0].ConversationId);
        Assert.AreEqual(30d, result.Items[0].Duration);
        Assert.AreEqual(true, result.Items[0].Incoming);
    }
    [TestMethod]
    public void ConversationDetails_FlattenTranscriptAndGroupedParticipants()
    {
        var result = CommunicationMapping.Details("""
        {"id":"conv1","video_recording":{"url":"https://example.com/video"},"audio_recording":{"url":"https://example.com/audio"},"account_ids":null,"opportunity_ids":null,
        "transcript":[{"id":"seg1","participant_id":"participant1","participant_name":"Alice","spoken_sentence":"Hello"}],
        "participants":{"internal":[],"external":{"others":[{"id":"participant1","contact_id":"contact1","organization_id":"company1","account_id":"account1","name":"Alice"}]}}}
        """, "conv1_sharetoken");
        Assert.AreEqual("Alice: Hello", result.TranscriptText);
        Assert.AreEqual("https://example.com/video", result.VideoUrl);
        Assert.AreEqual("https://example.com/audio", result.AudioUrl);
        Assert.AreEqual("contact1", result.Participants[0].ContactId);
        Assert.AreEqual("company1", result.Participants[0].CompanyId);
        Assert.AreEqual("account1", result.Participants[0].AccountId);
        Assert.AreEqual(0, result.AccountIds.Length);
        Assert.AreEqual(0, result.DealIds.Length);
    }
    [TestMethod]
    public void MissingTranscriptAndRecording_RemainEmptyOrNull()
    {
        var result = CommunicationMapping.Details("""{"id":"conv1","state":"failed","transcript":null,"participants":{"internal":[],"external":{}}}""", "conv1");
        Assert.AreEqual(0, result.Transcript.Length);
        Assert.AreEqual("", result.TranscriptText);
        Assert.IsNull(result.VideoUrl);
        Assert.AreEqual("failed", result.State);
        Assert.AreEqual(0, result.Participants.Length);
    }
    [TestMethod]
    public void WrongDetailIdAndWrongPages_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => CommunicationMapping.Details("""{"id":"other"}""", "conv1"));
        Assert.ThrowsException<PluginApplicationException>(() => CommunicationMapping.Page<CallResponse>("""{"phone_calls":[],"pagination":{"page":1}}""", "phone_calls", 2, x => x.CallId));
        Assert.ThrowsException<PluginApplicationException>(() => CommunicationMapping.Page<CallResponse>("""{"phone_calls":[{}]}""", "phone_calls", 1, x => x.CallId));
    }
    [TestMethod]
    public async Task BlankConversationId_IsRejectedBeforeNetworkCall()
    {
        var actions = new CommunicationActions(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", "offline-unused-key") } });
        await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() => actions.GetConversation(new()));
    }
}
[TestClass]
[TestCategory("Integration")]
public class CommunicationIntegrationTests
{
    private static CommunicationActions Actions()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Configure your local API key.");
        using var settings = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? settings.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Configure your local API key.");
        return new(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", key!) } });
    }
    [TestMethod]
    public async Task SearchCalls_ReturnsDialerResults()
    {
        var result = await Actions().SearchCalls(new() { MaximumResults = 5 });
        Assert.IsNotNull(result.Calls);
        Assert.IsTrue(result.ReturnedCalls <= 5);
        Assert.AreEqual(result.ReturnedCalls, result.Calls.Length);
        Assert.IsTrue(result.Calls.All(x => !string.IsNullOrWhiteSpace(x.CallId)));
        Assert.AreEqual(1, result.PagesRetrieved);
        Console.WriteLine($"Returned calls: {result.ReturnedCalls}; total: {result.TotalMatchingCalls}");
    }
    [TestMethod]
    public async Task SearchConversations_ReturnsSummaries()
    {
        var result = await Actions().SearchConversations(new() { MaximumResults = 5 });
        Assert.IsNotNull(result.Conversations);
        Assert.IsTrue(result.ReturnedConversations <= 5);
        Assert.AreEqual(result.ReturnedConversations, result.Conversations.Length);
        Assert.IsTrue(result.Conversations.All(x => !string.IsNullOrWhiteSpace(x.ConversationId)));
        Assert.AreEqual(1, result.PagesRetrieved);
        Console.WriteLine($"Returned conversations: {result.ReturnedConversations}; total: {result.TotalMatchingConversations}");
    }
    [TestMethod]
    public async Task GetConversation_ReturnsSelectedDetails()
    {
        // Explicitly run this test only when accepting the possible one-credit detail retrieval.
        var actions = Actions();
        var sample = await actions.SearchConversations(new() { MaximumResults = 25 });
        var selected = sample.Conversations.FirstOrDefault(x => x.CanAccess != false);
        if (selected == null) Assert.Inconclusive("No accessible conversation was found in the first 25 summaries. No detail request was made.");
        var result = await actions.GetConversation(new() { ConversationId = selected!.ConversationId! });
        Assert.AreEqual(selected.ConversationId, result.ConversationId);
        Assert.IsNotNull(result.Transcript);
        Assert.IsNotNull(result.Participants);
        Assert.IsNotNull(result.AccountIds);
    }
}
