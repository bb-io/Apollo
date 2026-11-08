# Apply bulk enrichment update (0.2.0)

Extract this ZIP directly into your existing Apollo solution folder, the folder containing Apollo.sln. Allow replacement of included source files. There is no additional Apollo directory in this update ZIP. Your local Tests.Apollo/appsettings.json is not included and is not overwritten.

Build and run offline tests, then the small bulk live test:

```powershell
dotnet build .\Apollo.sln
dotnet test .\Apollo.sln --filter "TestCategory=Offline"
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPeople_PreservesMatchedUnmatchedAndDuplicateInputs"
```

Keep your existing known-person and API key settings. The bulk endpoint requires its own key permission or a master key. The live test sends three inputs: your known person, an intentionally unknown person ID, and your known person again. This tests unmatched slots and duplicate handling together, and can consume credits. It expects exactly two matched rows.

Optional after the small test passes:

```powershell
dotnet test .\Apollo.sln --filter "FullyQualifiedName~EnrichPeople_SplitsElevenInputsAcrossBatches"
```

This sends eleven inputs in two requests (your known person first and last, with nine intentionally unknown IDs between). The known-person rows can consume credits. If either test fails, share the test error without sharing your API key or personal response data.

New action: Enrich people. Every input supports identifying fields plus an optional Source record ID. Results retain original one-based input positions, source IDs, unmatched entries and duplicate entries. See README.md for result-order assumptions, failure behavior and production limits.
