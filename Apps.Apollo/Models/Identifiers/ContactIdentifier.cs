using Apps.Apollo.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Apollo.Models.Identifiers;

public class ContactIdentifier
{
    [Display("Contact ID", Description = "Saved Apollo contact ID, not an Apollo database person ID.")]
    [DataSource(typeof(ContactDataHandler))]
    public string ContactId { get; set; } = string.Empty;
}
