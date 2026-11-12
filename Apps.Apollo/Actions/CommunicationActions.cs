using System.Globalization;
using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Identifiers;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Actions;

[ActionList("Calls and conversations")]
public class CommunicationActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int ConversationsPageSize = 25;
    private const int ConversationsMaximumResultsLimit = 10000;

    [Action("Search calls", Description = "Search dialer calls without fetching conversation details. Requires a master API key.")]
    public async Task<SearchCallsResponse> SearchCalls([ActionParameter] SearchCallsRequest input)
    {
        InputValidator.ValidateRange(input.StartDate, input.EndDate, "Start date", "End date");
        InputValidator.ValidateRange(input.MinimumDuration, input.MaximumDuration, "Minimum duration", "Maximum duration");
        var maximumResults = InputValidator.GetMaximumResults(input.MaximumResults);

        var result = await Client.Paginate<CallsPageDto, CallResponse>(
            (page, pageSize) => CreateSearchCallsRequest(input, page, pageSize),
            x => x.Calls,
            maximumResults);

        return new()
        {
            Calls = result.Items,
            ReturnedCalls = result.Items.Count,
            TotalMatchingCalls = result.TotalEntries
        };
    }

    [Action("Search conversations", Description = "Search conversation summaries by type, account, contacts, companies, tags or time range, without fetching transcripts.")]
    public async Task<SearchConversationsResponse> SearchConversations([ActionParameter] SearchConversationsRequest input)
    {
        InputValidator.ValidateRange(input.StartTime, input.EndTime, "Start time", "End time");
        var maximumResults = InputValidator.GetMaximumResults(input.MaximumResults, ConversationsMaximumResultsLimit);

        var result = await Client.Paginate<ConversationsPageDto, ConversationResponse>(
            (page, pageSize) => CreateSearchConversationsRequest(input, page, pageSize),
            x => x.Conversations,
            maximumResults,
            ConversationsPageSize);

        return new()
        {
            Conversations = result.Items,
            ReturnedConversations = result.Items.Count,
            TotalMatchingConversations = result.TotalEntries
        };
    }

    [Action("Get conversation", Description = "Get the transcript, recording URLs and participants of a conversation. Can consume one credit when AI insights exist.")]
    public async Task<ConversationDetailsResponse> GetConversation([ActionParameter] ConversationIdentifier input)
    {
        if (string.IsNullOrWhiteSpace(input.ConversationId))
            throw new PluginMisconfigurationException("Please fill in 'Conversation ID'.");

        var request = new RestRequest($"conversations/{Uri.EscapeDataString(input.ConversationId.Trim())}");
        var conversation = await Client.ExecuteWithErrorHandling<ConversationDetailsDto>(request);

        return new(conversation);
    }

    private static RestRequest CreateSearchCallsRequest(SearchCallsRequest input, int page, int pageSize)
    {
        return new RestRequest("phone_calls/search")
            .AddOptionalQueryParameter("q_keywords", input.Keywords)
            .AddOptionalQueryParameter("inbound", input.Direction)
            .AddQueryParameterList("user_ids[]", input.UserIds)
            .AddQueryParameterList("phone_call_purpose_ids[]", input.PurposeIds)
            .AddQueryParameterList("phone_call_outcome_ids[]", input.OutcomeIds)
            .AddOptionalDateQueryParameter("date_range[min]", input.StartDate)
            .AddOptionalDateQueryParameter("date_range[max]", input.EndDate)
            .AddOptionalQueryParameter("duration[min]", input.MinimumDuration)
            .AddOptionalQueryParameter("duration[max]", input.MaximumDuration)
            .AddPageQueryParameters(page, pageSize);
    }

    private static RestRequest CreateSearchConversationsRequest(SearchConversationsRequest input, int page, int pageSize)
    {
        var body = new Dictionary<string, object>
            {
                ["page"] = page,
                ["num_fetch_result"] = pageSize
            }
            .AddIfNotEmpty("conversation_type", input.Type)
            .AddIfNotEmpty("account_id", input.AccountId)
            .AddIfNotEmpty("contact_ids", input.ContactIds)
            .AddIfNotEmpty("organization_ids", input.CompanyIds)
            .AddIfNotEmpty("tag_ids", input.TagIds);

        if (input.StartTime.HasValue || input.EndTime.HasValue)
            body["date_range"] = CreateConversationDateRange(input.StartTime, input.EndTime);

        return new RestRequest("conversations/search", Method.Post).AddJsonBody(body);
    }

    private static Dictionary<string, object> CreateConversationDateRange(DateTime? startTime, DateTime? endTime)
    {
        return new Dictionary<string, object>()
            .AddIfNotEmpty("start", startTime?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))
            .AddIfNotEmpty("end", endTime?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
    }
}
