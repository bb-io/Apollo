using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Apps.Apollo.Services;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo.Actions;
[ActionList("Emails")]
public class EmailActions(InvocationContext context) : Invocable(context)
{
    [Action("Search outreach emails", Description = "Search sequence outreach by keywords, sequences, senders, status or date range. Does not retrieve inbound reply bodies.")]
    public Task<SearchOutreachEmailsResponse> SearchOutreachEmails([ActionParameter] SearchOutreachEmailsRequest input)
    {
        var (filters, limit) = EmailMapping.Validate(input);
        return Client.SearchEmailsAsync(filters, limit);
    }
    [Action("Get email content", Description = "Retrieve sent outbound email content by message IDs in batches of ten. Preserve unavailable input positions; drafts and inbound replies are excluded.")]
    public async Task<GetEmailContentResponse> GetEmailContent([ActionParameter] GetEmailContentRequest input)
    {
        var (ids, format) = EmailMapping.ValidateContent(input);
        var content = new Dictionary<string, EmailContentResponse>(StringComparer.Ordinal);
        foreach (var batch in ids.Distinct(StringComparer.Ordinal).Chunk(10))
            foreach (var (id, email) in await Client.GetEmailContentAsync(batch, format)) content.Add(id, email);
        return EmailMapping.Restore(ids, content);
    }
}
