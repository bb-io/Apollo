using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class OrganizationEnrichmentDto
{
    [JsonProperty("organization")]
    public CompanyResponse? Organization { get; set; }
}
