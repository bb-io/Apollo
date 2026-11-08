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
    internal static Dictionary<string, CompanyResponse> IndexBatch(string[] requested, CompanyResponse?[] companies)
    {
        var allowed = requested.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var index = new Dictionary<string, CompanyResponse>(StringComparer.OrdinalIgnoreCase);
        foreach (var company in companies)
        {
            if (company == null) continue;
            if (string.IsNullOrWhiteSpace(company.CompanyId) || string.IsNullOrWhiteSpace(company.Domain))
                throw new PluginApplicationException("Apollo returned a company without an ID or domain. Stopped to avoid linking it to the wrong source record.");
            string domain;
            try { domain = Domain(company.Domain); }
            catch (PluginMisconfigurationException) { throw new PluginApplicationException("Apollo returned an unreadable company domain."); }
            if (!allowed.Contains(domain))
                throw new PluginApplicationException("Apollo returned a company domain different from all requested domains. It may be a renamed company or domain alias. Use Enrich company for that record and review the result before syncing it.");
            if (index.TryGetValue(domain, out var previous) && previous.CompanyId != company.CompanyId)
                throw new PluginApplicationException("Apollo returned multiple company IDs for one domain. Review the company before syncing it.");
            index[domain] = company;
        }
        return index;
    }
    internal static EnrichCompaniesResponse RestoreRows(CompanyEnrichmentInput[] inputs, string[] domains, Dictionary<string, CompanyResponse> companies)
    {
        var results = domains.Select((domain, i) => new CompanyEnrichmentResult
        {
            InputPosition = i + 1, SourceRecordId = inputs[i].SourceRecordId, RequestedDomain = domain,
            MatchFound = companies.ContainsKey(domain), Company = companies.GetValueOrDefault(domain)
        }).ToArray();
        var matched = results.Count(x => x.MatchFound);
        return new() { Results = results, RequestedCompanies = inputs.Length, UniqueDomains = domains.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            MatchedCompanies = matched, UnmatchedCompanies = inputs.Length - matched };
    }
}
