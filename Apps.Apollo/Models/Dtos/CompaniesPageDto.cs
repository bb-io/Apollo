using Apps.Apollo.Models.Responses;
using Newtonsoft.Json;

namespace Apps.Apollo.Models.Dtos;

public class CompaniesPageDto : PageDto
{
    [JsonProperty("organizations")]
    public List<CompanyResponse> Organizations { get; set; } = [];

    [JsonProperty("accounts")]
    public List<AccountDto> Accounts { get; set; } = [];

    public IEnumerable<CompanyResponse> GetCompanies()
    {
        return (Accounts ?? []).Select(x => x.ToCompanyResponse()).Concat(Organizations ?? []);
    }
}
