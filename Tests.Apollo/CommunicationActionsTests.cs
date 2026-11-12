using Apps.Apollo.Actions;
using Apps.Apollo.Models.Identifiers;
using Apps.Apollo.Models.Requests;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class CommunicationActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchCalls_WithoutFilters_ReturnsCalls()
    {
        var actions = new CommunicationActions(InvocationContext);

        var result = await actions.SearchCalls(new SearchCallsRequest());

        PrintJsonResult(result);
        Assert.AreEqual(result.TotalMatchingCalls, result.ReturnedCalls);
    }

    [TestMethod]
    public async Task SearchCalls_WithFilters_ReturnsCalls()
    {
        var actions = new CommunicationActions(InvocationContext);

        var result = await actions.SearchCalls(new SearchCallsRequest
        {
            Direction = "outgoing",
            StartDate = DateTime.UtcNow.AddYears(-2),
            EndDate = DateTime.UtcNow,
            MinimumDuration = 0,
            MaximumDuration = 3600
        });

        PrintJsonResult(result);
        Assert.IsNotNull(result.Calls);
    }

    [TestMethod]
    public async Task SearchConversations_WithFilters_ReturnsConversations()
    {
        var actions = new CommunicationActions(InvocationContext);

        var result = await actions.SearchConversations(new SearchConversationsRequest
        {
            Type = "video_conference",
            StartTime = DateTime.UtcNow.AddYears(-1),
            EndTime = DateTime.UtcNow
        });

        PrintJsonResult(result);
        Assert.IsNotNull(result.Conversations);
    }

    [TestMethod]
    public async Task GetConversation_FirstFoundConversation_ReturnsDetails()
    {
        var actions = new CommunicationActions(InvocationContext);
        var conversations = await actions.SearchConversations(new SearchConversationsRequest { MaximumResults = 1 });

        if (conversations.Conversations.Count == 0)
            Assert.Inconclusive("The test account has no conversations.");

        var conversationId = conversations.Conversations[0].ConversationId;
        var result = await actions.GetConversation(new ConversationIdentifier { ConversationId = conversationId });

        PrintJsonResult(result);
        Assert.AreEqual(conversationId, result.ConversationId);
    }
}
