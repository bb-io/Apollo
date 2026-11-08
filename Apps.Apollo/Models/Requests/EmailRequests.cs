using Apps.Apollo.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
namespace Apps.Apollo.Models.Requests;
public class SearchOutreachEmailsRequest
{
    [Display("Keywords", Description = "Search Apollo outreach content; not an exact contact or recipient filter.")] public string? Keywords { get; set; }
    [Display("Sequence IDs")] public string[]? SequenceIds { get; set; }
    [Display("Sender user IDs")] public string[]? UserIds { get; set; }
    [Display("Email statuses"), StaticDataSource(typeof(EmailStatusDataSourceHandler))] public string[]? Statuses { get; set; }
    [Display("Start date", Description = "Supply both dates; the end date must be later. Only the calendar date is sent to Apollo.")] public DateTime? StartDate { get; set; }
    [Display("End date")] public DateTime? EndDate { get; set; }
    [Display("Date basis", Description = "Defaults to Sent date when a date range is supplied."), StaticDataSource(typeof(EmailDateBasisDataSourceHandler))] public string? DateBasis { get; set; }
    [Display("Maximum results", Description = "Defaults to 100; maximum 50,000. Blank filters search all visible outreach emails up to this limit.")] public int? MaximumResults { get; set; }
}
public class GetEmailContentRequest
{
    [Display("Email message IDs", Description = "Apollo outreach message IDs, not contact, sequence or provider message IDs. Missing or unsent messages return Content available=false.")]
    public string[] EmailMessageIds { get; set; } = [];
    [Display("Body format", Description = "Defaults to plain text."), StaticDataSource(typeof(EmailBodyFormatDataSourceHandler))] public string? BodyFormat { get; set; }
}
