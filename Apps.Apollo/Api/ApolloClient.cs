using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Apps.Apollo.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json.Linq;
using Polly;
using RestSharp;
namespace Apps.Apollo.Api;
public partial class ApolloClient : BlackBirdRestClient
{
    public ApolloClient(IEnumerable<AuthenticationCredentialsProvider> credentials) : base(new RestClientOptions
        { BaseUrl = new Uri("https://api.apollo.io/api/v1/"), Timeout = TimeSpan.FromSeconds(60) })
    {
        var key = credentials.FirstOrDefault(x => x.KeyName == CredsNames.ApiKey)?.Value;
        if (string.IsNullOrWhiteSpace(key)) throw new PluginMisconfigurationException("Enter an Apollo API key in the connection.");
        this.AddDefaultHeader("x-api-key", key.Trim());
        this.AddDefaultHeader("Accept", "application/json");
    }
    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var clock = Stopwatch.StartNew();
        var policy = Policy.HandleResult<RestResponse>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, (attempt, result, _) =>
            {
                var delay = RetryDelay(result.Result, attempt);
                if (clock.Elapsed + delay > TimeSpan.FromSeconds(90)) throw CreateException(result.Result);
                return delay;
            }, (_, _, _, _) => Task.CompletedTask);
        var response = await policy.ExecuteAsync(() => ExecuteAsync(request));
        if (!response.IsSuccessStatusCode) throw CreateException(response);
        return response;
    }
    private static TimeSpan RetryDelay(RestResponse response, int attempt)
    {
        var header = response.Headers?.FirstOrDefault(h => string.Equals(h.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        if (RetryConditionHeaderValue.TryParse(header, out var parsed))
            return parsed.Delta ?? (parsed.Date > DateTimeOffset.UtcNow ? parsed.Date.Value - DateTimeOffset.UtcNow : TimeSpan.Zero);
        try
        {
            var error = JObject.Parse(response.Content ?? "{}")["error_details"];
            if (error?["code"]?.ToString() == "USAGE.RATE_LIMIT.PLATFORM_RATE_LIMIT")
            {
                var seconds = error["suggestions"]?.Children().Select(x => (double?)x["retry_after_seconds"]).FirstOrDefault(x => x.HasValue);
                return TimeSpan.FromSeconds(Math.Max(0, seconds ?? 60));
            }
        }
        catch (Newtonsoft.Json.JsonException) { }
        return TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)) + TimeSpan.FromMilliseconds(Random.Shared.Next(500));
    }
    public Exception CreateException(RestResponse response) => ConfigureErrorException(response);
    protected override Exception ConfigureErrorException(RestResponse response)
    {
        string? message = null, code = null;
        try
        {
            var error = JObject.Parse(response.Content ?? "{}")["error_details"];
            message = (string?)error?["message"]; code = (string?)error?["code"];
        }
        catch (Newtonsoft.Json.JsonException) { }
        var detail = string.IsNullOrWhiteSpace(message) ? "Apollo could not complete the request." : message;
        if (!string.IsNullOrWhiteSpace(code)) detail += $" ({code})";
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new PluginMisconfigurationException("Apollo rejected the API key. Check the connection credentials."),
            HttpStatusCode.Forbidden => new PluginMisconfigurationException("Apollo denied access. Check the API key endpoint permissions and your plan. " + detail),
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => new PluginMisconfigurationException("Check the action inputs. " + detail),
            HttpStatusCode.NotFound => new PluginMisconfigurationException("Apollo could not find the requested resource. Check its ID. " + detail),
            HttpStatusCode.TooManyRequests => new PluginApplicationException("Apollo's rate limit was reached. Retry this Flight later. " + detail),
            _ => new PluginApplicationException($"Apollo request failed (HTTP {(int)response.StatusCode}). " + detail)
        };
    }
}
