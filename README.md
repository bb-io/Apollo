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

<!-- end docs -->
