using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Identifiers;

public class ConversationIdentifier
{
    [Display("Conversation ID", Description = "Use an ID from 'Search conversations'. Can consume one credit if AI insights exist.")]
    public string ConversationId { get; set; } = string.Empty;
}
