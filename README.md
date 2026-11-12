# Blackbird.io Apollo [Beta]

Connect Apollo sales intelligence to Blackbird workflows. Search saved contacts, companies and outreach activity; enrich people and companies; retrieve available conversation details.

<!-- begin docs -->

## Before setting up

Create an Apollo API key with permission for the actions you intend to use. Call search requires a master API key. Account plan, record access and endpoint permissions still apply.

## Connecting

In Blackbird, select **Apollo [Beta]**, click **Connect**, and enter your **API key**. Connection validation checks authentication; individual actions can still require additional permissions.

## Actions

Optional inputs are added using **+** under **Advanced** in the Bird editor. Search actions retrieve all pages automatically up to **Maximum results** (100 by default).

### People

- **Enrich person** Match a person and output their email, company and profile information. Can consume credits.
  - Identify the person with **Person ID**, **Email address**, **LinkedIn URL**, **Full name**, or both **First name** and **Last name**.
  - **Company domain** / **Company name**: Improve matching.
  - **Include personal emails**: Defaults to false.

### Companies

- **Enrich company** Find a company by its domain, website or LinkedIn company URL and output its details. Consumes one credit.
  - Fill in **Company domain**, **Website URL** or **LinkedIn company URL**. **Company name** alone is not supported.
- **Search companies** Search companies by name, domain, location, industry or size. Each retrieved page of 100 companies consumes one credit.
  - At least one filter is required.
  - **Minimum employees** and **Maximum employees** must be used together.

### Contacts

- **Search contacts** Search your saved contacts by keywords or contact lists. Does not search the general people database.
- **Get contact** Get a saved contact with its profile, email, company, phone and list information.
- **Search contact lists** Search lists of saved contacts. Account and sequence lists are excluded.

### Emails

- **Search outreach emails** Search sequence outreach emails by keywords, sequences, senders, status or date range. Inbound replies are not included.
  - **Date basis**: Sent date by default; Scheduled date is also available.
- **Get email content** Get the content of sent outbound emails by their message IDs. Drafts and inbound replies are not included.
  - Results keep the order of the input IDs; unknown or unsent messages are marked as unavailable.
  - **Body format**: Plain text by default; HTML is also supported.

### Calls and conversations

- **Search calls** Search dialer calls without fetching conversation details. Requires a master API key.
  - **Minimum duration** / **Maximum duration**: Call duration in seconds.
- **Search conversations** Search conversation summaries by type, account, contacts, companies, tags or time range, without fetching transcripts.
  - **Company IDs**: Apollo database organization IDs, not saved account IDs.
- **Get conversation** Get the transcript, recording URLs and participants of a conversation. Can consume one credit when AI insights exist.

<!-- end docs -->
