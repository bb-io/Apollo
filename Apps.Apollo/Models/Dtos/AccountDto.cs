using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class AccountDto
{
    [JsonProperty("id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonProperty("organization_id")]
    public string? OrganizationId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("website_url")]
    public string? WebsiteUrl { get; set; }

    [JsonProperty("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [JsonProperty("primary_domain")]
    public string? PrimaryDomain { get; set; }

    [JsonProperty("domain")]
    public string? Domain { get; set; }

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("founded_year")]
    public int? FoundedYear { get; set; }

    public CompanyResponse ToCompanyResponse()
    {
        return new()
        {
            CompanyId = OrganizationId,
            AccountId = AccountId,
            Name = Name,
            WebsiteUrl = WebsiteUrl,
            LinkedinUrl = LinkedinUrl,
            Domain = PrimaryDomain ?? Domain,
            Phone = Phone,
            FoundedYear = FoundedYear
        };
    }
}
