using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
namespace Apps.Apollo.Api;
public partial class ApolloClient
{
    public async Task<CompanyResponse?> EnrichCompanyAsync(Dictionary<string, string> details)
    {
        var request = new RestRequest("organizations/enrich", Method.Get);
        foreach (var (key, value) in details) request.AddQueryParameter(key, value);
        var response = await ExecuteWithErrorHandling(request);
        try
        {
            var payload = JObject.Parse(response.Content ?? "");
            if (payload["error_details"] is JObject) throw CreateException(response);
            if (!payload.ContainsKey("organization")) throw new PluginApplicationException("Apollo omitted the company result. Review before retrying enrichment.");
            return payload["organization"]?.Type == JTokenType.Null ? null : payload["organization"]?.ToObject<CompanyResponse>();
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable company data. Review before retrying enrichment."); }
    }
    public async Task<CompanyResponse?[]> EnrichCompaniesAsync(string[] domains)
    {
        var request = new RestRequest("organizations/bulk_enrich", Method.Post);
        foreach (var domain in domains) request.AddQueryParameter("domains[]", domain);
        var response = await ExecuteWithErrorHandling(request);
        try
        {
            var payload = JObject.Parse(response.Content ?? "");
            if (payload["error_details"] is JObject) throw CreateException(response);
            if ((string?)payload["status"] != "success" || payload["organizations"] is not JArray organizations)
                throw new PluginApplicationException("Apollo did not return successful bulk company results. Review before retrying enrichment.");
            if (payload["total_requested_domains"]?.Type == JTokenType.Integer && (int)payload["total_requested_domains"]! != domains.Length)
                throw new PluginApplicationException("Apollo returned an unexpected requested-domain count. Review before retrying enrichment.");
            return organizations.ToObject<CompanyResponse?[]>()!;
        }
        catch (JsonException) { throw new PluginApplicationException("Apollo returned unreadable bulk company data. Review before retrying enrichment."); }
    }
}
