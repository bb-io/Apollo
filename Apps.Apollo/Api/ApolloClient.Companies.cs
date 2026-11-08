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
}
