using Blackbird.Applications.Sdk.Common;
namespace Apps.Apollo.Models.Requests;
public class EnrichPeopleRequest
{
    [Display("People", Description = "People to enrich. Sent in batches of up to ten, in the supplied order.")]
    public PersonEnrichmentInput[] People { get; set; } = [];
    [Display("Include personal emails", Description = "Defaults to false. Applies to all people and may consume Apollo credits.")]
    public bool? RevealPersonalEmails { get; set; }
}

public class PersonEnrichmentInput : PersonIdentityInput
{
    [Display("Source record ID", Description = "Optional external record ID, returned unchanged and not sent to Apollo.")]
    public string? SourceRecordId { get; set; }
}
