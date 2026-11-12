using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class ContactsPageDto : PageDto
{
    [JsonProperty("contacts")]
    public List<ContactResponse> Contacts { get; set; } = [];
}
