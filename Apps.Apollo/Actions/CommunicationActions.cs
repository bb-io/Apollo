using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo.Actions;
[ActionList("Calls and conversations")]
public class CommunicationActions(InvocationContext context) : Invocable(context)
{
    [Action("Search calls", Description = "Search dialer calls and output available call information without fetching conversation details. Requires a master API key.")]
    public Task<SearchCallsResponse> SearchCalls([ActionParameter] SearchCallsRequest input)
        => Client.SearchCallsAsync(CommunicationMapping.Calls(input), CommunicationMapping.Limit(input.MaximumResults, 50000));
    [Action("Search conversations", Description = "Search conversation summaries by type, account, contacts, companies, tags or time range, without fetching transcripts or consuming detail-retrieval credits.")]
    public Task<SearchConversationsResponse> SearchConversations([ActionParameter] SearchConversationsRequest input)
        => Client.SearchConversationsAsync(CommunicationMapping.Conversations(input), CommunicationMapping.Limit(input.MaximumResults, 10000));
    [Action("Get conversation", Description = "Retrieve a conversation's available transcript, recording URLs and participants. May consume one credit when AI insights exist.")]
    public Task<ConversationDetailsResponse> GetConversation([ActionParameter] ConversationIdentifier input)
    {
        if (string.IsNullOrWhiteSpace(input.ConversationId)) throw new PluginMisconfigurationException("Supply a Conversation ID from Search conversations.");
        return Client.GetConversationAsync(input.ConversationId.Trim());
    }
}
