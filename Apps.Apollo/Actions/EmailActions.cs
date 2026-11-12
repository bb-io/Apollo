using Apps.Apollo.Extensions;
using Apps.Apollo.Models.Dtos;
using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Actions;

[ActionList("Emails")]
public class EmailActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    private const int EmailContentBatchSize = 10;
    private const string DefaultDateBasis = "completed_at";
    private const string DefaultBodyFormat = "plain";

    [Action("Search outreach emails", Description = "Search sequence outreach emails by keywords, sequences, senders, status or date range. Inbound replies are not included.")]
    public async Task<SearchOutreachEmailsResponse> SearchOutreachEmails([ActionParameter] SearchOutreachEmailsRequest input)
    {
        InputValidator.ValidateRange(input.StartDate, input.EndDate, "Start date", "End date");
        var maximumResults = InputValidator.GetMaximumResults(input.MaximumResults);

        var result = await Client.Paginate<OutreachEmailsPageDto, OutreachEmailResponse>(
            (page, pageSize) => CreateSearchOutreachEmailsRequest(input, page, pageSize),
            x => x.Emails,
            maximumResults);

        return new()
        {
            Emails = result.Items,
            ReturnedEmails = result.Items.Count
        };
    }

    [Action("Get email content", Description = "Get the content of sent outbound emails by their message IDs. Drafts and inbound replies are not included.")]
    public async Task<GetEmailContentResponse> GetEmailContent([ActionParameter] GetEmailContentRequest input)
    {
        var messageIds = input.EmailMessageIds.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

        if (messageIds.Count == 0)
            throw new PluginMisconfigurationException("Please fill in at least one 'Email message ID'.");

        var emails = await GetEmailContents(messageIds.Distinct(), input.BodyFormat ?? DefaultBodyFormat);
        var results = messageIds
            .Select((id, index) => new EmailContentResult
            {
                InputPosition = index + 1,
                MessageId = id,
                ContentAvailable = emails.ContainsKey(id),
                Email = emails.GetValueOrDefault(id)
            })
            .ToList();

        return new()
        {
            Results = results,
            RequestedEmails = results.Count,
            AvailableEmails = results.Count(x => x.ContentAvailable),
            UnavailableEmails = results.Count(x => !x.ContentAvailable)
        };
    }

    private async Task<Dictionary<string, EmailContentResponse>> GetEmailContents(IEnumerable<string> messageIds,
        string bodyFormat)
    {
        var emails = new List<EmailContentResponse>();

        foreach (var batch in messageIds.Chunk(EmailContentBatchSize))
        {
            var request = new RestRequest("emailer_messages/get_content", Method.Post)
                .AddJsonBody(new { ids = batch, body_format = bodyFormat });

            var response = await Client.ExecuteWithErrorHandling<EmailContentListDto>(request);
            emails.AddRange(response.Emails);
        }

        return emails.DistinctBy(x => x.MessageId).ToDictionary(x => x.MessageId);
    }

    private static RestRequest CreateSearchOutreachEmailsRequest(SearchOutreachEmailsRequest input, int page,
        int pageSize)
    {
        var request = new RestRequest("emailer_messages/search")
            .AddOptionalQueryParameter("q_keywords", input.Keywords)
            .AddQueryParameterList("emailer_campaign_ids[]", input.SequenceIds)
            .AddQueryParameterList("user_ids[]", input.UserIds)
            .AddQueryParameterList("emailer_message_stats[]", input.Statuses)
            .AddOptionalDateQueryParameter("emailer_message_date_range[min]", input.StartDate)
            .AddOptionalDateQueryParameter("emailer_message_date_range[max]", input.EndDate)
            .AddPageQueryParameters(page, pageSize);

        if (input.StartDate.HasValue || input.EndDate.HasValue)
            request.AddQueryParameter("emailer_message_date_range_mode", input.DateBasis ?? DefaultDateBasis);

        return request;
    }
}
