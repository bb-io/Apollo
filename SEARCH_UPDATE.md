# Company search update — 0.4.0

Apply after company enrichment 0.3.0. Extract directly into the existing folder containing Apollo.sln and allow replacements. Your local appsettings.json is not included or overwritten.

Added Search companies: optional name/domain/location/industry/employee-range filters, automatic pagination, a default limit of 100 and a hard ceiling of 50,000. At least one filter is required. Search returns candidates without automatically enriching or selecting them.

No new test settings are required. Your API key needs api/v1/mixed_companies/search permission or a master key.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_ByNameReturnsCandidates"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_ByDomainsReturnsKnownCompanies"
```

Each live smoke test retrieves at most one paid page. Optional after those pass:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_PaginatesToMaximumResults"
```

The optional test requests 101 results for software across two pages; sufficient Apollo search visibility is required. Live search may consume credits per page. No individual enrichment requests are made by search.

Superseded by the 0.4.1 fix: Search returns companies from both organizations and accounts, with separate Company ID and Account ID. Dedicated account-specific filters remain planned. See README.md for the page-budget and limited-result behavior.
