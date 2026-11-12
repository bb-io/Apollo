using System.Globalization;
using RestSharp;

namespace Apps.Apollo.Extensions;

public static class RestRequestExtensions
{
    public static RestRequest AddOptionalQueryParameter(this RestRequest request, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            request.AddQueryParameter(name, value.Trim());

        return request;
    }

    public static RestRequest AddOptionalQueryParameter(this RestRequest request, string name, int? value)
    {
        return request.AddOptionalQueryParameter(name, value?.ToString(CultureInfo.InvariantCulture));
    }

    public static RestRequest AddOptionalDateQueryParameter(this RestRequest request, string name, DateTime? value)
    {
        return request.AddOptionalQueryParameter(name, value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    public static RestRequest AddQueryParameterList(this RestRequest request, string name, IEnumerable<string>? values)
    {
        foreach (var value in values.CleanValues())
            request.AddQueryParameter(name, value);

        return request;
    }

    public static RestRequest AddPageQueryParameters(this RestRequest request, int page, int pageSize)
    {
        return request
            .AddQueryParameter("page", page)
            .AddQueryParameter("per_page", pageSize);
    }
}
