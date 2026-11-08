using Apps.Apollo.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
namespace Apps.Apollo.Services;
internal static class PeopleEnrichmentMapping
{
    internal static Dictionary<string, string> ToDetails(PersonIdentityInput? input)
    {
        if (input == null) throw new PluginMisconfigurationException("The person cannot be empty. Supply identifying information.");
        if (new[] { input.PersonId, input.Email, input.LinkedinUrl, input.Name }.All(string.IsNullOrWhiteSpace)
            && (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName)))
            throw new PluginMisconfigurationException("Provide a person ID, email address, LinkedIn URL, full name, or both first and last names. Add a company domain or name to improve matching.");
        var values = new Dictionary<string, string?>
        {
            ["id"] = input.PersonId, ["email"] = input.Email, ["linkedin_url"] = input.LinkedinUrl,
            ["name"] = input.Name, ["first_name"] = input.FirstName, ["last_name"] = input.LastName,
            ["domain"] = NormalizeDomain(input.Domain), ["organization_name"] = input.OrganizationName
        };
        return values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToDictionary(x => x.Key, x => x.Value!.Trim());
    }
    private static string? NormalizeDomain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        if (!Uri.TryCreate(text.Contains("://") ? text : "https://" + text, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(uri.UserInfo) || !uri.Host.Contains('.'))
            throw new PluginMisconfigurationException("Enter a valid company domain, such as apollo.io, or a company website URL.");
        return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host;
    }
}
