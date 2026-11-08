using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Responses;
public class EnrichPeopleResponse
{
    [Display("Results")] public PersonEnrichmentResult[] Results { get; set; } = [];
    [Display("Requested people")] public int RequestedPeople { get; set; }
    [Display("Matched people", Description = "Number of matched inputs, including repeated inputs; not Apollo's billing count.")]
    public int MatchedPeople { get; set; }
    [Display("Unmatched people")] public int UnmatchedPeople { get; set; }
    [Display("Credits consumed", Description = "Sum reported by Apollo across all batches. Blank if any batch omits this value.")]
    public decimal? CreditsConsumed { get; set; }
}
public class PersonEnrichmentResult
{
    [Display("Input position", Description = "One-based position in the original People collection.")] public int InputPosition { get; set; }
    [Display("Source record ID")] public string? SourceRecordId { get; set; }
    [Display("Match found")] public bool MatchFound { get; set; }
    [Display("Person")] public PersonResponse? Person { get; set; }
}
