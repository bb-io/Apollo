# Saved contacts and lists update — 0.5.0

Apply after the company search saved-account fix (0.4.1). Extract directly into your existing Apollo solution folder, allowing replacements. No local appsettings.json is included or overwritten.

Added Search contacts, Get contact, Search contact lists and searchable dropdowns for contacts/contact lists. Search filters by optional keywords and list IDs, automatically pages to the requested limit, and reads saved contacts without extra detail/enrichment calls for every contact. Lists are filtered to contact lists. IDs remain distinct across contacts, people, companies and accounts.

No new configuration fields are required. Your key needs contacts/search, contacts/show and labels/index permissions, or a master key.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_ReturnsSavedContacts"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetContact_ReturnsSameSavedContact"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContactLists_ReturnsContactLists"
```

These tests are read-only and use existing records; Get contact's test automatically searches for one saved contact before retrieving it. No new fixture IDs are required. With an empty workspace the contact tests are inconclusive; no records are created automatically. Empty contact-list results are valid.

Optional next checks (after the above pass):

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_ByListReturnsMembers"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~ContactDropdowns_ReturnUniqueItems"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_PaginatesWhenEnoughSavedContactsExist"
```

List filtering needs a list on one of the first 100 contacts. The pagination check requires at least 101 contacts. See README.md for paging, dropdown limits and API/runtime limitations.
