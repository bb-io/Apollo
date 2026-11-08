# Validation — calls and conversations 0.7.0

- App and test source compiled successfully with direct Roslyn compilation against .NET 8 reference assemblies and existing dependency binaries. System.Runtime cross-target unification warnings were emitted. Verify normal NuGet restore/build locally; this environment previously failed normal MSBuild process inspection/package-cache checks.
- All 52 offline MSTest checks passed via direct invocation of compiled methods and real assertions (not a standard VSTest run).
- Nine new checks cover documented call filters/duration units/direction; bad ranges; independent contact/account/company filters; explicit-offset conversion to UTC; call and conversation identifier separation; direct conversation response parsing; grouped participants and transcript text; missing transcripts/recordings; shared IDs; wrong IDs/pages; and blank detail-ID validation.
- Live tests are included for all three actions: call search, conversation summary search and single selected conversation retrieval. They have not run here; local credentials remain on the user's machine.
- Get conversation is separate because it may consume one credit for records with AI insights. Search does not call Get conversation automatically.
- Call search requires a master API key. The ambiguous contact_label_ids call filter is intentionally omitted pending live verification. API errors are surfaced, not silently skipped.
- Conversation page size 25 and maximum results 10,000 are conservative app choices, not claimed API limits. Calls use at most 100 per page, with a 500-page app guard. Cross-page/filter behavior is not yet live-verified for these new endpoints.
- User-reported validation through 0.6.0: all 43 offline checks; outbound search/content and eleven-ID HTML batch retrieval; all contact/list/dropdown/pagination checks; company search/pagination and all enrichment/batch-boundary tests passed locally.
- Recording URLs are returned without downloads. Unmodeled AI insight fields are not promised. Blackbird editor/runtime checks, production icon and OAuth/distribution review remain outstanding.
