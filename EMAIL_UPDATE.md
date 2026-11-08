# Outbound outreach email update — 0.6.0

Apply after contacts/lists 0.5.0. Extract directly into the Apollo solution folder and allow replacements. No local appsettings.json is included or overwritten.

Added Search outreach emails and Get email content. Search supports keywords, sequence IDs, sender user IDs, statuses and sent/scheduled date ranges, with automatic bounded pagination. Content retrieval accepts IDs, deduplicates/batches them ten at a time, matches responses by ID and restores every original input, including duplicates and unavailable emails.

Only completed outbound emails are available from the content endpoint; inbound replies, drafts and unsent/failed messages are omitted. This is not full email correspondence sync. Read-only actions/tests do not send messages or create/update records.

No new config fields are needed. The key needs emailer_messages/search and emailer_messages/get_content permissions, or master-key access.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchOutreachEmails_ReturnsMessages"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetEmailContent_ReturnsSentEmailAndPreservesMissingId"
```

The content test automatically finds an existing delivered sequence email. If none is visible, it reports inconclusive; nothing is sent. Search prints only aggregate counts, never email bodies or recipient details. Keep credentials and correspondence local.

Optional after the above pass:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetEmailContent_SplitsElevenUniqueIds"
```

This verifies the ten-ID batch limit, HTML format and omission of unavailable IDs. See README.md for the exact input/output, pagination fallback and API limitations.
