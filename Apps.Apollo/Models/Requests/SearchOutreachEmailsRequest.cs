using Apps.Apollo.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Apollo.Models.Requests;

public class SearchOutreachEmailsRequest
{
    [Display("Keywords", Description = "Search outreach content. This is not an exact contact or recipient filter.")]
    public string? Keywords { get; set; }

    [Display("Sequence IDs")]
    public IEnumerable<string>? SequenceIds { get; set; }

    [Display("Sender user IDs")]
    public IEnumerable<string>? UserIds { get; set; }

    [Display("Email statuses")]
    [StaticDataSource(typeof(EmailStatusDataHandler))]
    public IEnumerable<string>? Statuses { get; set; }

    [Display("Start date")]
    public DateTime? StartDate { get; set; }

    [Display("End date")]
    public DateTime? EndDate { get; set; }

    [Display("Date basis", Description = "Defaults to 'Sent date' when a date range is set.")]
    [StaticDataSource(typeof(EmailDateBasisDataHandler))]
    public string? DateBasis { get; set; }

    [Display("Maximum results", Description = "Defaults to 100, maximum 50,000.")]
    public int? MaximumResults { get; set; }
}
