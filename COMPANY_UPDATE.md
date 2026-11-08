# Company enrichment update — 0.3.0

Apply after the bulk people update (0.2.0). Extract this ZIP directly into the existing folder containing Apollo.sln; allow replacement of included files. No appsettings.json is included, so your API key and test person configuration remain local.

Added Enrich company and Enrich companies. Single-company enrichment accepts domain, website URL or LinkedIn company URL, plus an optional name hint. Bulk accepts a collection of domains and optional source record IDs. It deduplicates domains globally, batches up to ten unique domains, maps results by domain and restores every input row. An unrecognized canonical/alias domain causes an explicit error for review rather than a guessed match.

Run these in your Apollo solution folder:

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompany_ReturnsApollo"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompanies_PreservesDuplicatesAndUnmatchedDomains"
```

No new config fields are required. Your API key needs organizations/enrich and organizations/bulk_enrich permissions, or a master key. The live tests use apollo.io and microsoft.com and may consume credits.

Optional after those pass:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompanies_SplitsElevenUniqueDomains"
```

This exercises two API requests for eleven unique domains. See README.md for the input/output and failure behavior. Company search and name-only lookup are not part of this update.
