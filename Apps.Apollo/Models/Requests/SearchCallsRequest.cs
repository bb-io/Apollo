using Apps.Apollo.Handlers.Static;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.Apollo.Models.Requests;

public class SearchCallsRequest
{
    [Display("Keywords")]
    public string? Keywords { get; set; }

    [Display("Direction")]
    [StaticDataSource(typeof(CallDirectionDataHandler))]
    public string? Direction { get; set; }

    [Display("User IDs")]
    public IEnumerable<string>? UserIds { get; set; }

    [Display("Call purpose IDs", Description = "Find purpose IDs in the call filter URL in Apollo.")]
    public IEnumerable<string>? PurposeIds { get; set; }

    [Display("Call outcome IDs", Description = "Find outcome IDs in the disposition filter URL in Apollo.")]
    public IEnumerable<string>? OutcomeIds { get; set; }

    [Display("Start date")]
    public DateTime? StartDate { get; set; }

    [Display("End date")]
    public DateTime? EndDate { get; set; }

    [Display("Minimum duration", Description = "Call duration in seconds.")]
    public int? MinimumDuration { get; set; }

    [Display("Maximum duration", Description = "Call duration in seconds.")]
    public int? MaximumDuration { get; set; }

    [Display("Maximum results", Description = "Defaults to 100, maximum 50,000.")]
    public int? MaximumResults { get; set; }
}
