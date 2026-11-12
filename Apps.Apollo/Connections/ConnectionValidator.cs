using Apps.Apollo.Api;
using Apps.Apollo.Models.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Apollo.Connections;

public class ConnectionValidator(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new ApolloClient(authenticationCredentialsProviders);
            var health = await client.ExecuteWithErrorHandling<AuthHealthDto>(new RestRequest("auth/health"));

            return new()
            {
                IsValid = health.IsLoggedIn,
                Message = health.IsLoggedIn ? "Success" : "The API key is invalid."
            };
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[ApolloConnectionValidator] Connection validation failed: {ex.Message}", []);

            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}
