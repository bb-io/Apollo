using Blackbird.Applications.Sdk.Common;

namespace Apps.Apollo.Models.Responses;

public class SearchConversationsResponse
{
    [Display("Conversations")]
    public List<ConversationResponse> Conversations { get; set; } = [];

    [Display("Returned conversations")]
    public int ReturnedConversations { get; set; }

    [Display("Total matching conversations")]
    public long? TotalMatchingConversations { get; set; }
}
