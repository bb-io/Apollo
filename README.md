# Blackbird.io Apollo [Beta]

Connect Apollo sales intelligence to Blackbird workflows. Search saved contacts, companies and outreach activity; enrich people and companies; retrieve available conversation details.

<!-- begin docs -->

## Before setting up

Create an Apollo API key with permission for the actions you intend to use. Call search requires a master API key. Account plan, record access and endpoint permissions still apply. This app uses API key authentication; OAuth is not implemented.

## Connecting

In Blackbird, select **Apollo [Beta]** (or **Apollo [Beta]_Dev** in the internal test environment), click **Connect**, and enter your **API key**. Connection validation checks authentication; individual actions can still require additional permissions.

## Actions

Optional inputs are added using **+** under **Advanced** in the Bird editor. Shared input names are described once below and retain the same meaning wherever available. Required collection and ID inputs appear directly in their actions.

### People

- **Enrich person** Match a person and output available email, company and profile information. Supply identifying details; enrichment may consume credits.
  - **Person ID**: Database person ID, distinct from a saved Contact ID.
  - **Email address**: Known email used to identify the person.
  - **LinkedIn URL**: Person profile URL.
  - **Full name**: Use with a company domain or company name for better matching.
  - **First name**: Supply with Last name when Full name is absent.
  - **Last name**: Supply with First name.
  - **Company domain**: Bare domain or website URL, for example blackbird.io.
  - **Company name**: Company matching hint.
  - **Include personal emails**: Defaults to false. Applies to all rows in Enrich people when enabled there.

- **Enrich people** Enrich multiple people in batches of ten, preserving input positions and source record IDs. May consume credits.
  - **Source record ID**: Optional per-row external ID, such as a Notion page ID; preserved in outputs and never sent to the service.

### Companies

- **Enrich company** Find a company using its domain, website or LinkedIn company URL and output available company details. May consume credits.
  - **Website URL**: Company website URL.
  - **LinkedIn company URL**: Company profile URL. Supply a domain, website or LinkedIn company URL; company name alone is insufficient.

- **Enrich companies** Enrich multiple companies by domain in batches of ten, preserving source rows and enriching repeated domains once. May consume credits.

- **Search companies** Search company candidates by name, domain and other filters, up to Maximum results. May consume credits per page.
  - **Company domains**: Multiple domains or website URLs; at most 1,000 unique domains.
  - **Headquarters locations**: Multiple cities, states or countries.
  - **Industries**: Multiple exact industry names.
  - **Minimum employees**: Supply both employee bounds.
  - **Maximum employees**: Upper bound for employee filtering.
  - **Maximum results**: Default 100; range 1–50,000 for searches except conversations, which allows up to 10,000. Add this input under Advanced to change the total cap; pages are retrieved automatically.

### Contacts

- **Search contacts** Search your saved contacts by keywords or contact lists, up to Maximum results. Does not search the general people database or enrich missing details.
  - **Keywords**: Search saved-contact names, emails, job titles or company names. In other searches, keywords apply to that action’s records; they are not an exact recipient filter.
  - **List IDs**: Multiple contact lists. Contacts can belong to any selected list.

- **Get contact** Retrieve a saved contact by Contact ID and output available profile, email, company, phone and list information.

- **Search contact lists** Search lists for saved contacts, excluding account and sequence lists.
  - **List name**: Case-insensitive part of a contact-list name.

### Emails

- **Search outreach emails** Search sequence outreach by keywords, sequences, senders, status or date range. Does not retrieve inbound reply bodies.
  - **Sequence IDs**: Multiple sequence IDs.
  - **Sender user IDs**: Multiple sender IDs.
  - **Email statuses**: Multiple supported statuses; use Delivered to search sent messages. Completed is an output status.
  - **Start date**: Supply both calendar dates, with End date later than Start date. Also applies to call search.
  - **End date**: End calendar date.
  - **Date basis**: Sent date by default; Scheduled date is also available.

- **Get email content** Retrieve sent outbound email content by message IDs in batches of ten. Preserve unavailable input positions; drafts and inbound replies are excluded.
  - **Body format**: Plain text by default; HTML is also supported.

### Calls and conversations

- **Search calls** Search dialer calls and output available call information without fetching conversation details. Requires a master API key.
  - **Direction**: Incoming or outgoing.
  - **User IDs**: Multiple call-owner user IDs.
  - **Call purpose IDs**: Multiple IDs from the call filter URL.
  - **Call outcome IDs**: Multiple IDs from the disposition filter URL.
  - **Minimum duration**: Seconds; supply both duration bounds, with minimum at least zero.
  - **Maximum duration**: Seconds; must exceed Minimum duration.

- **Search conversations** Search conversation summaries by type, account, contacts, companies, tags or time range, without fetching transcripts or consuming detail-retrieval credits.
  - **Conversation type**: Video meeting or phone call.
  - **Account ID**: Saved company account ID.
  - **Contact IDs**: Multiple saved contact IDs.
  - **Company IDs**: Multiple database organization IDs, distinct from Account IDs.
  - **Tag IDs**: Multiple conversation tag IDs.
  - **Start time**: Supply both ISO 8601 timestamps with Z or an explicit timezone offset.
  - **End time**: Must be later than Start time; timestamps are normalized to UTC.

- **Get conversation** Retrieve a conversation's available transcript, recording URLs and participants. May consume one credit when AI insights exist.

## Matching and enrichment

Search contacts searches your team's **saved contacts**, not the general people database. An empty result does not establish that the person is absent from Apollo. Keywords can produce multiple candidates; verify the company or LinkedIn URL before selecting a contact. Get contact uses a saved Contact ID, while Enrich person uses a database Person ID. These identifiers are not interchangeable.

For a Notion-to-email workflow, first check whether Notion already holds an acceptable email. If needed, search saved contacts and inspect the matching contact's email and status. If no suitable saved result exists, use Enrich person with a LinkedIn URL, Person ID, or a full name plus company domain. For example, supply Full name and Company domain together. Check Match found, the returned identity, email and email status before updating Notion. A match does not guarantee an available or verified email. Missing match confidence stays blank; review low or unknown confidence. The app does not save contacts or update Notion automatically.

Enrich person requires a Person ID, email, LinkedIn URL, full name, or both first and last names. Company details improve matching. Include personal emails defaults to false and does not disable work-email enrichment. Phone and waterfall enrichment are not enabled by these actions.

Enrich company requires a domain, website URL or LinkedIn company URL; use Search companies for name-only discovery. Search outputs are candidates and can contain fewer fields than enrichment outputs. Search companies includes both database companies and saved accounts when present, with separate Company ID and Account ID fields. Unlinked saved accounts can have no Company ID. Total matching records is the API total before local deduplication.

## Collections, limits and credits

Searches default to 100 total results, not 100 pages. Maximum results controls the total cap and pagination is automatic. Blank filters search visible saved contacts, outreach emails, calls or conversations up to that cap; Search companies requires at least one filter. Company employee filters require both bounds. Results limited indicates that further or restricted results may exist; total counts remain blank when not supplied. Concurrent changes can affect pagination, so results are not a snapshot.

Bulk people enrichment submits up to ten rows per request. Outputs retain one-based Input position, Source record ID and unmatched or duplicate rows. Matching relies on the standard endpoint's positional response alignment, checked with known-ID and batch-boundary tests; name-only inputs cannot independently prove response ordering. Bulk company enrichment deduplicates normalized domains, submits up to ten unique domains per request and restores the original rows by domain. Alias or renamed domains that cannot be correlated safely should be enriched individually and reviewed.

Enrichment and company search may consume credits. Get conversation may consume one credit when AI insights exist. Bulk people Credits consumed uses only API-reported figures and stays blank when any batch omits its figure; counts are not billing estimates. A failed multi-request action produces no partial success output, but earlier requests may already have consumed credits. Errors identify completed work for bulk enrichment; review before rerunning. There is no durable batch-resume checkpoint.

## Outreach and conversations

Outreach search covers sequence emails, not a complete mailbox. Keywords are not an exact recipient or Contact ID filter. Get email content requires outreach Email message IDs, rather than provider, sequence or contact IDs. It retrieves completed outbound messages in batches of ten unique IDs, then restores duplicate and unavailable input positions. Content available means a message was included, even if its stored body is blank. Missing, inaccessible or unsent IDs produce Content available=false without an inferred reason. Neither action retrieves inbound reply bodies or sends email.

Search conversations outputs summaries without per-result detail requests. Use Get conversation separately with the chosen Conversation ID to obtain available transcript text, segments, participants and recording URLs. Processing or inaccessible content can lack transcripts or recordings. Recording URLs are not downloaded and remain subject to provider access and expiry. Call, conversation, contact, person, company and account IDs remain distinct.

Contact and list dropdowns show at most 30 suggestions. Search contact lists retrieves all matching contact lists without that suggestion cap; IDs can also be mapped directly. List counts are cached and may be stale.

## Error handling

Invalid inputs are rejected before requests where possible. Authentication, permissions, rate limits and service errors surface as action errors. HTTP 429 responses are retried up to three times with server retry guidance and a 90-second total retry-wait budget. Longer required waits fail for a later retry. Requests time out after 60 seconds; this is not an end-to-end action deadline. Network timeouts and server errors are not automatically retried, avoiding ambiguous repeated credit-consuming requests.

<!-- end docs -->

## Development and testing

Requires the .NET 8 SDK. From the solution directory:

```powershell
dotnet build .\Apollo.sln --configuration Release
dotnet test .\Apollo.sln --configuration Release --filter "TestCategory=Offline"
Copy-Item .\Tests.Apollo\appsettings.json.example .\Tests.Apollo\appsettings.json
```

For live tests, edit the local appsettings.json: set ConnectionDefinition.api_key and, for known-person tests, TestPerson.PersonId or Email plus ExpectedPersonId. APOLLO_API_KEY can override the configured key; the local configuration file is still required. Do not commit credentials. Missing configuration or unavailable sample records mark relevant tests inconclusive. Run individual integration tests deliberately, as enrichment and company search can consume credits.

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~Connection_"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_ReturnsSavedContacts"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPerson_ReturnsExpectedPerson"
```

The offline suite covers validation, mapping, pagination guards and batch correlation without API credentials. Live coverage includes known-person/company enrichment, duplicate and unmatched rows, eleven-input batch boundaries, company search, contact/list search and pagination, dropdowns, outbound content and communication searches. Get conversation requires an accessible sample and was previously skipped when none was available. The maintainer has also reported successful sandbox testing; this does not imply every optional filter or plan-dependent endpoint was exercised.

Pushes to develop run build/offline checks and dispatch Blackbird's central internal deployment workflow. The dispatcher does not wait for downstream completion; verify that central run and the app in the target environment. The pipeline appends _Dev to Product automatically. Production promotion follows a reviewed develop-to-main PR; merging triggers production delivery. Build/offline checks and deployment dispatch run independently. CI does not run credential-dependent integration tests.

## Current scope and release review

The current app retrieves and enriches data. It does not implement contact writes, general people prospect search, asynchronous phone/waterfall enrichment, inbound-mail sync or workflow events. Endpoint permissions, account plan and credits govern availability.

Before production promotion, replace the placeholder image/icon.png with an approved app icon and confirm the intended authentication/distribution requirements with engineering. Review action mappings and remaining account-specific edge cases in the test environment.

## API references

- [Apollo API reference](https://docs.apollo.io/reference/apollo-api)
- [People enrichment](https://docs.apollo.io/reference/people-enrichment)
- [Bulk people enrichment](https://docs.apollo.io/reference/bulk-people-enrichment)
- [Organization enrichment](https://docs.apollo.io/reference/organization-enrichment)
- [Bulk organization enrichment](https://docs.apollo.io/reference/bulk-organization-enrichment)
- [Organization search](https://docs.apollo.io/reference/organization-search)
- [Contact search](https://docs.apollo.io/reference/search-for-contacts)
- [Contact retrieval](https://docs.apollo.io/reference/view-a-contact)
- [Contact lists](https://docs.apollo.io/reference/get-a-list-of-all-lists)
- [Outreach email search](https://docs.apollo.io/reference/search-for-outreach-emails)
- [Call search](https://docs.apollo.io/reference/search-for-calls)
- [Conversation search](https://docs.apollo.io/reference/search-conversations)
- [Conversation details](https://docs.apollo.io/reference/get-conversations-info)
