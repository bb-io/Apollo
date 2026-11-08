using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
namespace Tests.Apollo;
[TestClass]
[TestCategory("Offline")]
public class EmailTests
{
    [TestMethod]
    public void SearchFilters_MapDocumentedParameters()
    {
        var (filters, limit) = EmailMapping.Validate(new() { Keywords = " test ", SequenceIds = ["s1", "s1"], UserIds = ["u1"], Statuses = ["delivered"], StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 3) });
        Assert.AreEqual(100, limit);
        Assert.AreEqual("test", filters.Single(x => x.Key == "q_keywords").Value);
        Assert.AreEqual("s1", filters.Single(x => x.Key == "emailer_campaign_ids[]").Value);
        Assert.AreEqual("u1", filters.Single(x => x.Key == "user_ids[]").Value);
        Assert.AreEqual("delivered", filters.Single(x => x.Key == "emailer_message_stats[]").Value);
        Assert.AreEqual("completed_at", filters.Single(x => x.Key == "emailer_message_date_range_mode").Value);
        Assert.AreEqual("2026-01-01", filters.Single(x => x.Key == "emailer_message_date_range[min]").Value);
    }
    [TestMethod]
    public void BadStatusesDatesAndLimits_AreRejected()
    {
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.Validate(new() { Statuses = ["completed"] }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.Validate(new() { StartDate = DateTime.Today }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.Validate(new() { StartDate = DateTime.Today, EndDate = DateTime.Today }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.Validate(new() { MaximumResults = 50001 }));
    }
    [TestMethod]
    public void ContentInputs_KeepRepeatedIdsAndRejectUnsupportedFormats()
    {
        var (ids, format) = EmailMapping.ValidateContent(new() { EmailMessageIds = [" id1 ", "id1", "missing"] });
        CollectionAssert.AreEqual(new[] { "id1", "id1", "missing" }, ids);
        Assert.AreEqual("plain", format);
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.ValidateContent(new() { EmailMessageIds = ["id"], BodyFormat = "HTML" }));
        Assert.ThrowsException<PluginMisconfigurationException>(() => EmailMapping.ValidateContent(new() { EmailMessageIds = ["id", " "] }));
    }
    [TestMethod]
    public void ShuffledOmittedContent_RestoresOriginalIdsAndDuplicates()
    {
        var content = EmailMapping.Content("""
        {"emailer_messages":[{"id":"b","status":"completed","body_format":"plain","body":""},{"id":"a","status":"completed","body_format":"plain","body":"test","recipients":null}]}
        """, ["a", "missing", "b"], "plain");
        var result = EmailMapping.Restore(["a", "missing", "b", "a"], content);
        Assert.AreEqual(4, result.RequestedEmails);
        Assert.AreEqual(3, result.AvailableEmails);
        Assert.AreEqual(1, result.UnavailableEmails);
        Assert.IsNull(result.Results[1].Email);
        Assert.IsFalse(result.Results[1].ContentAvailable);
        Assert.AreEqual("test", result.Results[3].Email?.Body);
        Assert.AreEqual(4, result.Results[3].InputPosition);
        Assert.AreEqual(0, result.Results[0].Email!.Recipients.Length);
    }
    [TestMethod]
    public void NullHtmlBody_IsAvailableAndPreserved()
    {
        var content = EmailMapping.Content("""{"emailer_messages":[{"id":"a","status":"completed","body_format":"html","body":null}]}""", ["a"], "html");
        var row = EmailMapping.Restore(["a"], content).Results[0];
        Assert.IsTrue(row.ContentAvailable);
        Assert.IsNull(row.Email!.Body);
    }
    [TestMethod]
    public void UnexpectedIdsFormatsAndStatuses_AreRejected()
    {
        Assert.ThrowsException<PluginApplicationException>(() => EmailMapping.Content("""{"emailer_messages":[{"id":"other","status":"completed","body_format":"plain"}]}""", ["a"], "plain"));
        Assert.ThrowsException<PluginApplicationException>(() => EmailMapping.Content("""{"emailer_messages":[{"id":"a","status":"drafted","body_format":"plain"}]}""", ["a"], "plain"));
        Assert.ThrowsException<PluginApplicationException>(() => EmailMapping.Content("""{"emailer_messages":[{"id":"a","status":"completed","body_format":"html"}]}""", ["a"], "plain"));
    }
    [TestMethod]
    public void SearchMapping_KeepsIdentifiersSeparateAndMapsRecipients()
    {
        var page = EmailMapping.Search("""
        {"emailer_messages":[{"id":"message1","contact_id":"contact1","emailer_campaign_id":"sequence1","provider_message_id":"provider1","recipients":[{"email":"test@example.com","recipient_type_cd":"to"}]}],"pagination":{"page":1,"total_entries":1,"total_pages":1}}
        """, 1);
        var email = page.Emails[0];
        Assert.AreEqual("message1", email.MessageId);
        Assert.AreEqual("contact1", email.ContactId);
        Assert.AreEqual("sequence1", email.SequenceId);
        Assert.AreEqual("provider1", email.ProviderMessageId);
        Assert.AreEqual("to", email.Recipients[0].Type);
    }
    [TestMethod]
    public void EmptyEmailResultsAndWrongPages_AreHandled()
    {
        Assert.AreEqual(0, EmailMapping.Search("""{"emailer_messages":[]}""", 1).Emails.Length);
        Assert.AreEqual(0, EmailMapping.Content("""{"emailer_messages":[]}""", ["unknown"], "plain").Count);
        Assert.ThrowsException<PluginApplicationException>(() => EmailMapping.Search("""{"emailer_messages":[],"pagination":{"page":1}}""", 2));
        Assert.ThrowsException<PluginApplicationException>(() => EmailMapping.Search("{}", 1));
    }
}
[TestClass]
[TestCategory("Integration")]
public class EmailIntegrationTests
{
    private static EmailActions Actions()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) Assert.Inconclusive("Configure your local Apollo API key.");
        using var config = JsonDocument.Parse(File.ReadAllText(path));
        var key = Environment.GetEnvironmentVariable("APOLLO_API_KEY") ?? config.RootElement.GetProperty("ConnectionDefinition").GetProperty("api_key").GetString();
        if (string.IsNullOrWhiteSpace(key)) Assert.Inconclusive("Configure your local Apollo API key.");
        return new(new InvocationContext { AuthenticationCredentialsProviders = new[] { new AuthenticationCredentialsProvider("api_key", key!) } });
    }
    [TestMethod]
    public async Task SearchOutreachEmails_ReturnsMessages()
    {
        var result = await Actions().SearchOutreachEmails(new() { MaximumResults = 5 });
        Assert.IsNotNull(result.Emails);
        Assert.AreEqual(result.ReturnedEmails, result.Emails.Length);
        Assert.IsTrue(result.ReturnedEmails <= 5);
        Assert.AreEqual(1, result.PagesRetrieved);
        Assert.IsTrue(result.Emails.All(x => !string.IsNullOrWhiteSpace(x.MessageId)));
        Assert.AreEqual(result.ReturnedEmails, result.Emails.Select(x => x.MessageId).Distinct().Count());
        Console.WriteLine($"Returned outreach emails: {result.ReturnedEmails}; total: {result.TotalMatchingEmails}; limited: {result.ResultsLimited}");
    }
    [TestMethod]
    public async Task GetEmailContent_ReturnsSentEmailAndPreservesMissingId()
    {
        var actions = Actions();
        var sample = await actions.SearchOutreachEmails(new() { Statuses = ["delivered"], MaximumResults = 10 });
        if (sample.ReturnedEmails == 0) Assert.Inconclusive("No delivered sequence outreach emails are visible. An existing completed outbound sequence email is needed for this test; nothing is sent automatically.");
        var first = sample.Emails[0].MessageId!;
        var result = await actions.GetEmailContent(new() { EmailMessageIds = [first, "not-a-valid-apollo-id", first] });
        Assert.AreEqual(3, result.RequestedEmails);
        Assert.AreEqual(2, result.AvailableEmails, "Apollo did not return stored content for the delivered email selected by the test.");
        Assert.AreEqual(1, result.UnavailableEmails);
        Assert.AreEqual(first, result.Results[0].Email?.MessageId);
        Assert.AreEqual("plain", result.Results[0].Email?.BodyFormat);
        Assert.AreEqual("completed", result.Results[0].Email?.Status);
        Assert.IsFalse(result.Results[1].ContentAvailable);
        Assert.AreEqual(first, result.Results[2].Email?.MessageId);
    }
    [TestMethod]
    public async Task GetEmailContent_SplitsElevenUniqueIds()
    {
        var actions = Actions();
        var sample = await actions.SearchOutreachEmails(new() { Statuses = ["delivered"], MaximumResults = 1 });
        if (sample.ReturnedEmails == 0) Assert.Inconclusive("A delivered sequence outreach email is required.");
        var first = sample.Emails[0].MessageId!;
        var ids = new[] { first }.Concat(Enumerable.Range(1, 10).Select(i => $"unavailable-email-test-{i}")).ToArray();
        var result = await actions.GetEmailContent(new() { EmailMessageIds = ids, BodyFormat = "html" });
        Assert.AreEqual(11, result.RequestedEmails);
        Assert.AreEqual(1, result.AvailableEmails);
        Assert.AreEqual(10, result.UnavailableEmails);
        Assert.AreEqual("html", result.Results[0].Email?.BodyFormat);
        Assert.AreEqual(11, result.Results[10].InputPosition);
    }
}
