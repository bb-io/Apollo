using Blackbird.Applications.Sdk.Common;
using System.Net;
using Apps.Apollo.Api;
using Apps.Apollo.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;
using RestSharp;
namespace Apps.Apollo.Connections;
public class ConnectionValidator(InvocationContext context) : BaseInvocable(context), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        var credentials = authProviders.ToArray();
        if (!credentials.Any(x => x.KeyName == CredsNames.ApiKey && !string.IsNullOrWhiteSpace(x.Value)))
            return new() { IsValid = false, Message = "Enter your Apollo API key." };
        using var client = new ApolloClient(credentials);
        var response = await client.ExecuteAsync(new RestRequest("auth/health", Method.Get), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new() { IsValid = false, Message = "Apollo rejected the API key. Check or replace it." };
        if (!response.IsSuccessStatusCode) throw client.CreateException(response);
        JObject payload;
        try { payload = JObject.Parse(response.Content ?? ""); }
        catch { throw new PluginApplicationException("Apollo returned an unreadable authentication response. Try connecting again."); }
        if (payload["is_logged_in"]?.Value<bool>() == false)
            return new() { IsValid = false, Message = "Apollo did not authenticate this API key. Check or replace it." };
        if (payload["is_logged_in"]?.Value<bool>() != true)
            throw new PluginApplicationException("Apollo did not confirm authentication. Try connecting again.");
        return new() { IsValid = true, Message = "Connected to Apollo." };
    }
}
