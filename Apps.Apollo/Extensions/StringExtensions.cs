namespace Apps.Apollo.Extensions;

public static class StringExtensions
{
    public static List<string> CleanValues(this IEnumerable<string>? values)
    {
        return (values ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public static string ToBareDomain(this string value)
    {
        var trimmed = value.Trim();
        var url = trimmed.Contains("://") ? trimmed : $"https://{trimmed}";
        var host = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : trimmed;

        return host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
    }
}
