using Apps.Apollo.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Apollo.Models.Requests;

public class GetEmailContentRequest
{
    [Display("Email message IDs", Description = "Outreach message IDs from 'Search outreach emails'. Unsent or unknown messages are returned as unavailable.")]
    public IEnumerable<string> EmailMessageIds { get; set; } = [];

    [Display("Body format", Description = "Defaults to plain text.")]
    [StaticDataSource(typeof(EmailBodyFormatDataHandler))]
    public string? BodyFormat { get; set; }
}
