# Blackbird.io Apollo — starter build

Incremental Apollo app build: single/bulk person enrichment and single/bulk company enrichment, company search, saved-contact search/get, contact-list lookup and outbound outreach email retrieval, dialer call search and conversation search/details are implemented. This is not yet the complete planned app.

<!-- begin docs -->
## Before setting up

Use an Apollo API key with permission for `api/v1/people/match` (or a master key). API key authentication is implemented; OAuth is not implemented. Apollo documents OAuth for partner integrations, so confirm that distribution model before public release.

## Connecting

Add an Apollo connection in Blackbird and enter the API key. The validator calls `GET /api/v1/auth/health`. Authentication failures are distinguished from permissions, rate limits and service failures; transient failures surface as errors rather than successful validation.

## Actions

### Enrich person

Matches a person using a person ID, email, LinkedIn URL, full name, or first and last names. Company domain/name improves matching. Company URLs are normalized to bare domains. Inputs are sent as query parameters as defined by the supplied OpenAPI. Outputs include match found, match confidence, person and contact IDs, profile, email status, location and nested company data. A missing match returns `MatchFound=false` and `Person=null`; a missing confidence value is preserved rather than invented. Low-confidence results must be reviewed before updating external records.

Personal emails are optional and disabled by default. Phone and waterfall enrichment are not included in this batch. Enrichment can consume Apollo credits even when personal emails are disabled. No secondary profile request is made.

### Enrich people

Supply a collection of people with the same identity fields as Enrich person, plus an optional Source record ID for each row (for example, the Notion page ID). Personal email enrichment is a single optional setting for the entire action. All inputs are validated before the first API request. The action submits sequential batches of at most ten to `POST /api/v1/people/bulk_match`, with no per-person follow-up requests. Phone and waterfall enrichment are explicitly disabled.

Each result contains Input position (one-based across the full collection), Source record ID (unchanged), Match found and Person. Unmatched inputs remain in place with Person=null. Duplicate inputs remain separate result rows. Aggregate matched/unmatched counts count input rows; credits consumed is a decimal sum of Apollo-reported billing figures, or blank if any batch omits its figure. It is not inferred from match counts.

Apollo documents null slots for unmatched standard-enrichment entries. Positional alignment of matches with details is an integration assumption to verify with the live tests; the reference does not explicitly guarantee ordering. Result cardinality and supplied person IDs are checked before returning a batch. These checks cannot independently prove ordering for inputs identified only by name/email. Review low/unknown-confidence matches before syncing external records. Use person IDs where available.

On a failed batch the action stops, returns no partial success response and identifies the failed input range and number of previously completed inputs in its error. Earlier batches—and possibly the failed request—may already have consumed credits. Do not blindly retry the whole action. This version has no durable batch resume state. No personal data or credentials are logged by the action or tests.

### Enrich company

Supply a company domain, website URL or LinkedIn company URL; optionally add the company name as a matching hint. This action conservatively requires an identifier other than name: the supplied OpenAPI says name alone is unsupported, while the current web overview is inconsistent with its parameter descriptions. Use Search companies for name-only lookup.

Calls `GET /api/v1/organizations/enrich`. Domain input accepts bare domains and website URLs, normalizing case and removing a leading www. Website and LinkedIn inputs must be HTTP/HTTPS URLs; LinkedIn URLs must identify a company. A null organization result returns Match found=false; unreadable/missing result structures raise an error rather than masquerading as a no-match. A returned company without an ID raises an error. A single-company match may resolve to a canonical/renamed domain; review it before updating an external record.

Outputs include company ID, domain, website, LinkedIn URL, name, industry, employee estimate, description, corporate phone, location, founding year, revenue, funding and parent company ID when available. Optional financial figures are returned as numbers without assuming an undocumented currency. These are organization IDs, not saved Apollo account IDs.

### Enrich companies

Supply Companies, a collection of Company domain and optional Source record ID. Bulk enrichment in this version requires domains; website URLs in the domain field are normalized. It does not accept company-name-only or LinkedIn-only rows. All rows are validated before the first request. Normalized domains are deduplicated across the full action and sent sequentially to `POST /api/v1/organizations/bulk_enrich` through repeated `domains[]` query parameters, at most ten unique domains per request.

Returned companies are indexed by normalized primary_domain, independently of API result ordering or omitted/null unmatched entries. Original rows are then restored with one-based Input position, Source record ID, Requested domain, Match found and Company. Duplicates retain distinct source rows while avoiding repeat enrichment within the same action. Returned domains outside the requested set, missing IDs/domains and conflicting company IDs for one domain raise an error. Renamed/alias domains must be enriched individually and reviewed rather than guessed into a source row.

Matched/unmatched counts refer to original input rows; Unique domains reports normalized domain count. Credit usage is not inferred: the supplied company schema does not expose a credit total. On a failed batch, the action stops without partial success output and reports the batch number plus previously completed unique domains. Earlier and possibly current requests may already have consumed credits; there is no durable resume state. No extra per-company profile requests are made.

### Search companies

Search the Apollo company database using Company name (partial match), Company domains, Headquarters locations, exact Apollo industry names or an employee range. At least one filter is required. Employee-range filtering requires both bounds. Maximum results defaults to 100 and accepts 1–50,000. Domains accept URLs, normalize and deduplicate; at most 1,000 unique domains are sent. Names/locations/industries are trimmed; blank list values are removed.

Calls `POST /api/v1/mixed_companies/search` with documented query parameters. Pages are fetched sequentially with at most 100 records each. Page/per-page inputs are hidden from the workflow editor. There are no follow-up calls to enrich individual companies. Search returns candidate companies, not a selected match: review the name/domain and pass the chosen domain into enrichment when needed. A search profile can have fewer fields than an enriched profile; absent values remain blank.

Paid page requests are bounded by ceil(Maximum results / page size), up to Apollo's 500-page display ceiling. A small search cap uses a correspondingly small page size. Result IDs are deduplicated, wrong page metadata and repeated full company pages are rejected, and processing stops at the requested limit or end of pagination. Empty searches return an empty Companies array. If duplicate results reduce the unique company count, the action can return fewer than Maximum results rather than fetch additional paid pages beyond the budget.

Outputs: Companies, Returned companies, Total matching records when Apollo reports it, Pages retrieved, Results limited, and Saved accounts included. Both organizations and accounts from the same search response are included. Company ID always represents the database organization ID: an account maps organization_id to Company ID and its own id to the separate Account ID. An unlinked account keeps Company ID blank and remains usable by domain or Account ID. Account domain is read from primary_domain or domain. No company ID is invented and no extra detail/enrichment requests are made.

Results deduplicate by Company ID when available, otherwise by Account ID in a separate namespace. If the same organization appears in both arrays, the organization's profile is retained along with its first linked Account ID. Multiple saved accounts linked to one organization do not produce duplicate company rows. Distinct unlinked accounts are retained separately; they are not merged by potentially ambiguous domain/name matches.

Total matching records is Apollo's combined total before local deduplication. Results limited is true when more records may exist because of the result/page budget or Apollo partial-result access; it is not an exact remaining-result count. Saved accounts included means the response contained saved accounts, not that every company is saved in the workspace.

Failed searches produce an error rather than partial success output. Earlier/current requests may already have consumed credits, including when parsing fails. API errors use the shared retry/error handling. Dedicated saved-account search with account-specific filters remains a separate planned action.

### Search contacts

Searches saved contacts in your Apollo workspace, rather than prospecting the general people database. Optional Keywords can match names, emails, job titles or employers. Optional List IDs uses Apollo's contact_label_ids body field; multiple lists match contacts from any selected list, alongside keyword filtering. Blank filters export saved contacts up to Maximum results (default 100, range 1–50,000).

Calls `POST /api/v1/contacts/search` using a JSON dictionary body. Page size is at most 100, with up to 500 sequential pages. The page-request budget is ceil(Maximum results / page size). Sorts by contact_created_at ascending. Contact IDs are deduplicated, wrong/repeating pages are rejected, and empty results return an empty Contacts array. Results limited flags remaining/partial results when detectable. Pagination is not a snapshot: concurrent contact creation, deletion or list changes can affect offset-based results. This action is not yet an incremental sync or event.

Returns available contact profile, email/status, company name, all four distinct IDs, LinkedIn URL, location, timestamps, list IDs, available phone numbers and email unsubscribe state. It makes no detail/enrichment request for each contact. It does not reveal new phone/email data or automatically write to another CRM; use the returned collection in your Bird to map downstream fields.

### Get contact

Select a Contact ID through the searchable dropdown or map an existing saved contact ID. Calls `GET /api/v1/contacts/{contact_id}` with an escaped ID. Blank IDs are rejected locally and a returned ID mismatch raises an error. Contact ID is the workspace record ID; Person ID may be blank for manually created contacts. No ID is substituted for another identifier type.

### Search contact lists

Calls `GET /api/v1/labels` once. Apollo documents this as an all-lists endpoint with no pagination or search parameters. The action filters locally to modality=contacts, supports an optional case-insensitive List name substring, and returns unique List IDs, names and Apollo's cached contact counts. Counts may be stale. Account and sequence lists are excluded. Empty results return an empty collection.

### Contact dropdowns

Get contact has a dynamic contact dropdown using server-side keyword search and at most 30 results from one request. Search contacts has a dynamic list dropdown: one all-lists request, local name filtering, then at most 30 suggestions. The Search contact lists action returns all matching contact lists without that suggestion cap; IDs can also be mapped directly into the workflow. Dropdowns need the corresponding endpoint permissions. Cancellation is checked before and after retrieval; shared HTTP timeout/retry handling applies during the request. Editor/runtime interaction still requires testing in Blackbird.

### Search outreach emails

Searches emails created/sent through Apollo sequences using `GET /api/v1/emailer_messages/search`. Optional filters: Keywords, Sequence IDs, Sender user IDs, Email statuses and date range. Keywords are not an exact contact or recipient filter. There is no documented contact-ID filter on this endpoint, so this action does not add one or silently scan the workspace for a contact.

Date filters require both Start date and End date, with the end calendar date later than the start. Date basis defaults to Sent date (completed_at) and also supports Scheduled date (due_at). The action sends YYYY-MM-DD only; it does not apply an undocumented timezone conversion. Search status choices are validated against documented values. Use delivered as the sent-email search filter; completed is a response status, not a supported search-filter value.

Maximum results defaults to 100 and accepts 1–50,000. Blank filters retrieve visible outreach emails up to the cap. Pages are sequential, at most 100 records each, bounded by ceil(Maximum results / page size) and 500 pages. Pagination metadata is used when returned; Apollo's supplied example omits it, so short-page detection is the fallback. Total matching emails stays blank when not reported. Duplicate message IDs are removed and wrong/repeated pages raise an error.

Outputs include message/contact/sequence/account/user/email-account/provider identifiers kept distinct; subject, sender/recipient information, send/schedule/failure timestamps, status, failure reason and reply classification; and text/HTML bodies when included by search. Search bodies may be missing/unloaded; absence is not fabricated into complete content. There are no per-message content or enrichment requests during search.

### Get email content

Accepts a collection of Apollo Email message IDs and a Body format (plain text by default or HTML). Uses `POST /api/v1/emailer_messages/get_content` with an anonymous JSON body containing ids and body_format. Unique IDs are batched into groups of at most ten; this avoids Apollo silently ignoring IDs after the first ten. Identical IDs are requested once per action and restored to each original input position.

This endpoint returns only completed outbound emails sent by your team. Drafts, scheduled/failed messages, inbound replies, unknown IDs and inaccessible IDs are omitted by Apollo. Results are mapped by ID rather than array position. Every requested input gets one result with Input position, Email message ID, Content available and nullable Email content. Missing content is not treated as an API error and does not reveal why the ID was omitted. A matched email with a blank/null stored body remains Content available=true, with the original empty/null body preserved. Available/unavailable counts refer to input positions, including duplicate IDs.

Returned content has the subject, requested-format body, status, sent timestamp and recipients. Recipient types use the same output field for the search endpoint's recipient_type_cd and the content endpoint's type. Unexpected IDs, duplicate response entries, wrong format or non-completed content status raise an error to prevent incorrect downstream association. All batches must succeed before output is returned; this read-only action has no durable resume state.

These actions are not a full mailbox or inbound-reply sync. Reply classification is metadata, not the reply body. They never send an email, update a contact or write into an external CRM. Link downstream CRM activity records using message ID and available contact/account IDs, and map the selected content explicitly in your Bird.

### Search calls

Calls `GET /api/v1/phone_calls/search` to retrieve calls made or received through Apollo's dialer. This endpoint requires a master API key according to Apollo's current docs. Optional filters include Keywords, Direction, User IDs, Call purpose IDs, Call outcome IDs, paired calendar dates and paired duration bounds in seconds. Direction is sent as the documented string incoming/outgoing, while the returned Incoming call field is boolean. End date must be later than Start date; duration bounds require 0 <= minimum < maximum.

The docs use an ambiguous contact_label_ids[] parameter name while describing contacts and linking to user-ID lookup. This version deliberately does not expose that unverified contact/list filter. Purpose/outcome IDs can be taken from Apollo's filter URLs. Calls return independent Call ID, Conversation ID, Contact ID, Account ID and User ID plus available numbers, timestamps, duration, status, note text, recording URL and transcription status.

Maximum results defaults to 100 and permits 1–50,000. Pages contain at most 100 records with an app guard of 500 requests; IDs deduplicate, wrong/repeated pages fail, and empty results return an empty Calls array. Recording URLs are returned as metadata only: the action does not download recordings, request transcripts or retrieve linked conversation details.

### Search conversations

Calls `POST /api/v1/conversations/search` with a JSON dictionary. Optional filters include Conversation type (video meeting/phone call), Account ID, Contact IDs, Company IDs (organization IDs), Tag IDs and paired timestamp bounds. Times must be ISO 8601 with Z or an explicit offset and are normalized to UTC. End time must follow Start time. No undocumented topic/keyword or sort fields are invented.

Apollo uses num_fetch_result rather than per_page for this endpoint. The app uses 25 records per request as a conservative page size matching Apollo's examples; this is not asserted to be the API maximum. Maximum results defaults to 100 with an app-selected ceiling of 10,000. Retrieval is sequential with bounded page requests, ID deduplication and loop guards. Standard pagination metadata is used when present, with short-page fallback otherwise. Empty results return an empty Conversations array. Result limit/partial-access conditions are exposed through Results limited.

Search returns available topic, type, state, start time, duration, host/user ID, privacy/access metadata, participant names and account IDs. Search summaries do not include transcripts or recording URLs, and the action does not issue detail requests for each result. Search is documented as zero credits.

### Get conversation

Calls `GET /api/v1/conversations/{id}` for one ID from conversation search. Apollo supports an optional id_shareid; the output ID is checked against the base conversation ID. The detail response is a direct object, not a conversation wrapper. A matching ID is required before returning data.

Outputs include the available base conversation fields, audio/video recording URLs, transcript segments, combined transcript text, flattened participants, associated account/deal IDs and failure code. Transcript text preserves Apollo's sentence order and prefixes participant names when supplied. Segment timing units are not documented in the supplied schema, so this version does not guess their units or expose timing offsets. Participants are mapped from the documented internal/external groups; participant, contact, company and account IDs stay separate. Other unmodeled AI insight/scorecard fields are not implied to be available in this action.

Missing transcripts/participants/ID arrays are normalized to empty collections; absent recording URLs remain null. Processing state and failure code remain available when details have not been generated successfully. URLs are metadata only; access/expiry remains controlled by Apollo or the recording provider. No recording download or AI-summary generation is triggered by this action.

Get conversation may consume one credit when AI insights exist, and zero otherwise. The search action never invokes it automatically. All actions in this batch are read-only, and none records calls, initiates meetings or updates a CRM. Stable activity IDs can be mapped into downstream CRM deduplication; this batch does not implement an event or incremental checkpoint.

## Error handling

Reads `error_details.code` and `error_details.message`. Does not depend on retiring legacy error fields. Retries only HTTP 429, up to three times, honoring Retry-After and Apollo platform-rate-limit suggestions. Delays that exceed the remaining 90-second retry-wait budget fail immediately so the workflow can be retried later. No automatic retry of network timeouts or 5xx responses, avoiding ambiguous repeat credit-consuming requests. Per-request timeout is 60 seconds; the wait budget is not a hard end-to-end runtime limit.

<!-- end docs -->

## Local build and testing (PowerShell)

Install the .NET 8 SDK (Visual Studio 2022 17.8+ supports .NET 8). Extract this folder, open Apollo.sln, and restore NuGet packages.

```powershell
dotnet build .\Apollo.sln
Copy-Item .\Tests.Apollo\appsettings.json.example .\Tests.Apollo\appsettings.json
```

Edit `Tests.Apollo/appsettings.json` locally. Enter the API key under `ConnectionDefinition.api_key`, or set `APOLLO_API_KEY` in your local shell. Keep appsettings.json local; it is gitignored and excluded from the starter ZIP.

For the enrichment test, choose a real known Apollo person. Fill `PersonId` (Apollo database person ID, not contact ID) or `Email`, plus `ExpectedPersonId`. The test requires an actual match and exact expected ID; an empty result cannot pass. No personal data is printed by the tests.

```powershell
# Authentication first; does not run enrichment
dotnet test .\Apollo.sln --filter "FullyQualifiedName~Connection_"
# Then enrich one known person; may consume Apollo credits
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPerson_ReturnsExpectedPerson"
```

Missing local configuration marks live tests inconclusive. CI builds without credentials and does not run live tests. The icon is a placeholder: replace `Apps.Apollo/image/icon.png` with the approved Apollo icon before release. The central Blackbird release dispatcher is not configured; CI performs only a normal build until the repo and organization settings exist.

## Next implementation batches

Dedicated saved-account search/get; asynchronous phone enrichment; polling events only after verifying incremental API filters and Blackbird runtime support.

## Bulk enrichment tests

The new tests reuse your existing TestPerson and API key settings. Your key must allow `api/v1/people/bulk_match`; single-person permission alone is insufficient. The first test supplies the known person twice with an intentionally unknown ID in the middle. The optional second test submits eleven inputs to exercise the ten-person boundary. Both tests require two known-person result rows and preserved unmatched/source positions. Both may consume credits. No phone or personal email enrichment is requested.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPeople_PreservesMatchedUnmatchedAndDuplicateInputs"
# Optional: two API calls, eleven inputs
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPeople_SplitsElevenInputsAcrossBatches"
```

The collection input shape compiles against the SDK. Blackbird editor mapping and deployed execution still need runtime verification.

## Sources

- https://docs.apollo.io/reference/authentication
- https://docs.apollo.io/reference/people-enrichment
- The supplied apollo-rest-api.json

## Offline checks

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
```

See VALIDATION.md for the exact build/test status of this starter.

- https://docs.apollo.io/reference/bulk-people-enrichment

## Company enrichment tests

No new appsettings.json fields are needed. The live tests reuse your existing API key with the public example domains apollo.io and microsoft.com. Your key must permit organizations/enrich and organizations/bulk_enrich (or be a master key).

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompany_ReturnsApollo"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompanies_PreservesDuplicatesAndUnmatchedDomains"
# Optional: eleven unique domains across two requests
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichCompanies_SplitsElevenUniqueDomains"
```

The bulk smoke test uses apollo.io, a reserved .invalid domain, microsoft.com, and a duplicate Apollo domain. It checks duplicate restoration and unmatched handling without depending on result ordering. The boundary test uses two known domains and nine reserved .invalid domains. All live enrichment tests may consume Apollo credits.

- https://docs.apollo.io/reference/organization-enrichment
- https://docs.apollo.io/reference/bulk-organization-enrichment

## Company search tests

No config changes are required. Your local key must allow api/v1/mixed_companies/search or be a master key.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_ByNameReturnsCandidates"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_ByDomainsReturnsKnownCompanies"
# Optional pagination test: up to two paid pages
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCompanies_PaginatesToMaximumResults"
```

The first two live tests retrieve at most one page each. The optional test searches the broad name fragment software and requests 101 companies across two pages; it requires sufficient Apollo access to return those results. All live search calls may consume credits per page, even when used for tests.

- https://docs.apollo.io/reference/organization-search

## Contact tests

No new appsettings.json fields are required. Tests reuse your local key. Enable permissions for api/v1/contacts/search, api/v1/contacts/show and api/v1/labels/index, or use a master key. The endpoints are documented as zero-credit record retrieval and do not run enrichment. Tests are read-only and do not print contact details.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_ReturnsSavedContacts"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetContact_ReturnsSameSavedContact"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContactLists_ReturnsContactLists"
```

Get contact's test first retrieves one saved contact, then verifies that exact contact ID. With no visible saved contacts, contact-dependent tests are inconclusive rather than inventing or creating a record. An empty contact-list array is a valid response.

Optional checks:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_ByListReturnsMembers"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~ContactDropdowns_ReturnUniqueItems"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchContacts_PaginatesWhenEnoughSavedContactsExist"
```

List filtering selects a list ID from the first 100 saved contacts and checks returned memberships; it is inconclusive if those contacts have no lists. Pagination requires at least 101 saved contacts. Do not paste your key or personal contact data in debugging output.

- https://docs.apollo.io/reference/search-for-contacts
- https://docs.apollo.io/reference/view-a-contact
- https://docs.apollo.io/reference/get-a-list-of-all-lists

## Outreach email tests

No new local configuration fields are needed. Enable api/v1/emailer_messages/search and api/v1/emailer_messages/get_content permissions, or use a master key. The endpoints are documented as zero-credit read operations.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchOutreachEmails_ReturnsMessages"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetEmailContent_ReturnsSentEmailAndPreservesMissingId"
```

The content test selects an existing delivered outreach email, requests its content twice with an unavailable ID in between, and verifies exact ID/format/status association. It is inconclusive if no delivered sequence email is visible; no message is sent automatically. Search can validly return zero records and prints only counts, not subjects, recipients or bodies.

Optional content batch/HTML check after those pass:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetEmailContent_SplitsElevenUniqueIds"
```

This uses one existing sent email and ten intentionally unavailable IDs in two content requests; it verifies HTML format and missing-result preservation across the batch boundary.

- https://docs.apollo.io/reference/search-for-outreach-emails
- Supplied apollo-rest-api.json: POST /emailer_messages/get_content (request and response schemas, omitted-ID and sent-only behavior)

## Call and conversation tests

Use your existing local appsettings.json. Call search requires a master API key. Conversation endpoints need conversations/search and conversations/show permissions or master access. Feature/plan permissions are still enforced by Apollo; tests do not mask access-denied errors as passing results.

```powershell
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchCalls_ReturnsDialerResults"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~SearchConversations_ReturnsSummaries"
```

Both searches are documented as zero-credit operations. Empty collections are valid; tests print counts only. The next read-only test first searches up to 25 summaries, selects an accessible candidate and fetches its details. It may consume one credit when AI insights exist. With no accessible candidate it reports inconclusive and makes no detail request.

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~GetConversation_ReturnsSelectedDetails"
```

These tests are not a live cross-page test for calls or conversations; production pagination and optional filters still need account-specific verification with sufficient records. Get conversation does not require a transcript/recording to be present: processing/failed conversations may legitimately lack them. Correspondence and recording data are not printed by the tests.

- https://docs.apollo.io/reference/search-for-calls
- https://docs.apollo.io/reference/search-conversations
- https://docs.apollo.io/reference/get-conversations-info
