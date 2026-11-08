# Calls and conversations update — 0.7.0

Apply after outreach emails 0.6.0. Extract into the existing folder containing Apollo.sln; allow replacements. Your local appsettings.json is excluded and unchanged.

Added Search calls, Search conversations and Get conversation. Searches retrieve pages without per-record detail/enrichment calls. Conversation details are requested explicitly, returning available transcript text/segments, recording URLs and participant associations. Nothing is downloaded, sent, created or updated.

No new configuration fields are needed. Search calls requires an Apollo master API key. Conversation search/detail require conversations/search and conversations/show permissions or master access; plan/feature restrictions may also apply.

Run the searches first:

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCalls_ReturnsDialerResults"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchConversations_ReturnsSummaries"
```

Search tests are read-only, documentably zero-credit, and accept an empty workspace response. Counts are printed, not personal data.

Then run the detail test if there is an accessible conversation. It can consume one Apollo credit when AI insights exist:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetConversation_ReturnsSelectedDetails"
```

The test selects one candidate from the first 25 summaries. With no accessible candidate it is inconclusive and makes no detail call. A transcript or recording is not required for a valid returned conversation: failed/unprocessed recordings may lack them.

See README.md for the documented call-filter ambiguity deliberately omitted from this version, timezone handling, app pagination limits and the exact fields exposed. Cross-page search and filter behavior still need live account-specific verification beyond these smoke tests.
