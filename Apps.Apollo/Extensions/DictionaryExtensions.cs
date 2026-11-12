namespace Apps.Apollo.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<string, object> AddIfNotEmpty(this Dictionary<string, object> body, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            body[key] = value.Trim();

        return body;
    }

    public static Dictionary<string, object> AddIfNotEmpty(this Dictionary<string, object> body, string key,
        IEnumerable<string>? values)
    {
        var cleanValues = values.CleanValues();

        if (cleanValues.Count > 0)
            body[key] = cleanValues;

        return body;
    }
}
