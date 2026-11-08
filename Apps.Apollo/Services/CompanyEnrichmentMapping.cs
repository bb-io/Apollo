using Apps.Apollo.Models.Requests;
using Apps.Apollo.Models.Responses;
using Blackbird.Applications.Sdk.Common.Exceptions;
namespace Apps.Apollo.Services;
internal static class CompanyEnrichmentMapping
{
    internal static string Domain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new PluginMisconfigurationException("Enter a company domain, such as apollo.io.");
        var uri = Website(value.Contains("://") ? value : "https://" + value.Trim());
        var host = uri.IdnHost.ToLowerInvariant().TrimEnd('.');
        return host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;
    }
    private static Uri Website(string value)
    {
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")
            || !string.IsNullOrEmpty(uri.UserInfo) || Uri.CheckHostName(uri.IdnHost) != UriHostNameType.Dns || !uri.IdnHost.Contains('.'))
            throw new PluginMisconfigurationException("Enter a valid company domain or HTTP/HTTPS website URL, without an email address or credentials.");
        return uri;
    }
    internal static Dictionary<string, string> SingleDetails(EnrichCompanyRequest input)
    {
        if (new[] { input.Domain, input.Website, input.LinkedinUrl }.All(string.IsNullOrWhiteSpace))
            throw new PluginMisconfigurationException("Provide a company domain, website URL or LinkedIn company URL. A company name alone is not supported by this action; look up its domain first.");
        var details = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(input.Domain)) details["domain"] = Domain(input.Domain);
        if (!string.IsNullOrWhiteSpace(input.Website)) details["website"] = Website(input.Website).AbsoluteUri;
        if (!string.IsNullOrWhiteSpace(input.LinkedinUrl))
        {
            var uri = Website(input.LinkedinUrl);
            if (uri.IdnHost is not ("linkedin.com" or "www.linkedin.com") || !uri.AbsolutePath.StartsWith("/company/", StringComparison.OrdinalIgnoreCase)
                || uri.AbsolutePath[9..].Trim('/').Length == 0)
                throw new PluginMisconfigurationException("Enter a LinkedIn company URL such as https://www.linkedin.com/company/apolloio.");
            details["linkedin_url"] = input.LinkedinUrl.Trim();
        }
        if (!string.IsNullOrWhiteSpace(input.Name)) details["name"] = input.Name.Trim();
        return details;
    }
}
