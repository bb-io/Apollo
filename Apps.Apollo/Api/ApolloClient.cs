using System.Net;
using Apps.Apollo.Constants;
using Apps.Apollo.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Apollo.Api;

public class ApolloClient : BlackBirdRestClient
{
    private const int MaxRateLimitRetries = 3;
    private const int MaxPages = 500;
    private const int DefaultPageSize = 100;
    private const int MaxErrorMessageLength = 300;

    private static readonly AsyncRetryPolicy<RestResponse> RateLimitPolicy = Policy
        .HandleResult<RestResponse>(x => x.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            MaxRateLimitRetries,
            (attempt, result, _) => GetRetryDelay(result.Result, attempt),
            (_, _, _, _) => Task.CompletedTask);

    public ApolloClient(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new RestClientOptions
    {
        BaseUrl = new Uri("https://api.apollo.io/api/v1/"),
        Timeout = TimeSpan.FromSeconds(60)
    })
    {
        this.AddDefaultHeader("x-api-key", creds.Get(CredsNames.ApiKey).Value.Trim());
    }

    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await RateLimitPolicy.ExecuteAsync(() => ExecuteAsync(request));

        if (!response.IsSuccessStatusCode)
            throw ConfigureErrorException(response);

        return response;
    }

    public async Task<PaginatedResult<T>> Paginate<TPage, T>(
        Func<int, int, RestRequest> createPageRequest,
        Func<TPage, IEnumerable<T>?> getPageItems,
        int maximumResults,
        int maxPageSize = DefaultPageSize) where TPage : PageDto
    {
        var pageSize = Math.Min(maxPageSize, maximumResults);
        var items = new List<T>();
        long? totalEntries = null;

        for (var page = 1; page <= MaxPages && items.Count < maximumResults; page++)
        {
            var pageResponse = await ExecuteWithErrorHandling<TPage>(createPageRequest(page, pageSize));
            var pageItems = getPageItems(pageResponse)?.ToList() ?? [];

            totalEntries = pageResponse.Pagination?.TotalEntries;
            items.AddRange(pageItems.Take(maximumResults - items.Count));

            if (IsLastPage(pageResponse.Pagination, page, pageItems.Count, pageSize))
                break;
        }

        return new(items, totalEntries);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var message = GetErrorMessage(response);

        return response.StatusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden
            or HttpStatusCode.NotFound
            or HttpStatusCode.UnprocessableEntity
            ? new PluginMisconfigurationException(message)
            : new PluginApplicationException(message);
    }

    private static bool IsLastPage(PaginationDto? pagination, int page, int pageItemCount, int pageSize)
    {
        return pagination?.TotalPages is { } totalPages
            ? page >= totalPages
            : pageItemCount < pageSize;
    }

    private static string GetErrorMessage(RestResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Content))
            return response.ErrorMessage ?? $"Apollo request failed with status code {(int)response.StatusCode}.";

        var error = TryDeserializeError(response.Content);
        var message = error?.ErrorDetails?.Message ?? error?.Error ?? response.Content;

        return message.Length > MaxErrorMessageLength
            ? message[..MaxErrorMessageLength] + "..."
            : message;
    }

    private static ErrorDto? TryDeserializeError(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<ErrorDto>(content);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static TimeSpan GetRetryDelay(RestResponse response, int attempt)
    {
        var retryAfter = response.Headers?
            .FirstOrDefault(x => string.Equals(x.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?
            .Value?.ToString();

        return int.TryParse(retryAfter, out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(500));
    }
}
