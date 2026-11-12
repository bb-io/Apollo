using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchCallsResponse
{
    [Display("Calls")]
    public List<CallResponse> Calls { get; set; } = [];

    [Display("Returned calls")]
    public int ReturnedCalls { get; set; }

    [Display("Total matching calls")]
    public long? TotalMatchingCalls { get; set; }
}
