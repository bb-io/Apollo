using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Apollo.Utils;

public static class InputValidator
{
    public const int DefaultMaximumResults = 100;
    public const int MaximumResultsLimit = 50000;

    public static int GetMaximumResults(int? maximumResults, int limit = MaximumResultsLimit)
    {
        var value = maximumResults ?? DefaultMaximumResults;

        if (value < 1 || value > limit)
            throw new PluginMisconfigurationException($"'Maximum results' must be between 1 and {limit}.");

        return value;
    }

    public static void ValidateRange<T>(T? from, T? to, string fromName, string toName) where T : struct, IComparable<T>
    {
        if (from.HasValue && to.HasValue && to.Value.CompareTo(from.Value) < 0)
            throw new PluginMisconfigurationException($"'{toName}' must be greater than or equal to '{fromName}'.");
    }

    public static void ValidateBothOrNone<T>(T? first, T? second, string firstName, string secondName) where T : struct
    {
        if (first.HasValue != second.HasValue)
            throw new PluginMisconfigurationException($"Please fill in both '{firstName}' and '{secondName}', or leave both empty.");
    }
}
