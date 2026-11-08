# Company search saved-account fix — 0.4.1

Cause: Apollo returned both expected domain matches in accounts rather than organizations. Version 0.4.0 excluded accounts, returning zero usable companies despite total_entries=2. This update includes both arrays.

Extract directly into your Apollo solution folder and allow replacements. Your local appsettings.json is not included or overwritten.

Account IDs and company IDs remain distinct. Account id maps to Account ID; organization_id maps to Company ID, which stays blank for unlinked accounts. Domain uses primary_domain with fallback to domain. Results from both arrays are deduplicated by Company ID or, for unlinked accounts, a separate Account ID key. No extra network requests are made.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_ByDomainsReturnsKnownCompanies" --logger "console;verbosity=detailed"
```

The domain test still requires both apollo.io and microsoft.com; assertions have not been weakened. It now reports SavedAccountsIncluded rather than SavedAccountsExcluded. No configuration changes are needed. The live test repeats one search page and may consume Apollo credits.
