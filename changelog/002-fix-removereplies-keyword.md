---
id: 002
date: 2026-06-01
package-under-test: 26.5.0
type: fix
---

# Fix RemoveReplies ToolCatalog keyword (Pitfall #15)

## What changed

`Fixtures/ToolCatalog.cs:32` — change `Resolve("reply", required: "remove")` to
`Resolve("replies", required: "remove")` for the `RemoveReplies` resolver.

## Why

The `Contains` substring check is literal. The wire name `remove_replies` does
NOT contain the substring `reply` because `replies` is spelled
`r-e-p-l-i-e-s` — there's no `y` after the `l`. Pairing the keyword `"reply"`
with `required: "remove"` meant the resolver searched for tool names containing
BOTH `reply` AND `remove`, which matched NO tool on the wire.

Symptom on CI: `ToolDiscoveryTests.ListTools_ExposesAllTenAnnotationTools`
and `RemoveRepliesTests.RemoveReplies_BlankPdf_ReportsNoReplies` both threw

```
System.InvalidOperationException : No tool with name containing 'reply' and
'remove'. Found: import_annotations, add_reply, export_annotations,
update_annotation, generate_pages_preview, get_document_info, add_annotation,
remove_replies, get_annotations, remove_annotations
```

Run [#26742076482](https://github.com/groupdocs-annotation/GroupDocs.Annotation.Mcp.Tests/actions/runs/26742076482)
failed on all three OSes for this reason.

`AddReply` resolver is unaffected — its wire name is `add_reply` (singular),
which DOES contain `reply` as a substring.

## Why no version bump

Per the
[Version + changelog on MCP repo change](https://github.com/groupdocs-annotation/GroupDocs.Annotation.Mcp/blob/master/build/dependencies.props)
rule, version bumps are only required when the **product** repo changes. This
fix is tests-repo-only — the published `GroupDocs.Annotation.Mcp 26.5.0`
NuGet is unchanged. `Directory.Build.props` still pins `McpPackageVersion=26.5.0`.

## Verification

`dotnet test src/GroupDocs.Annotation.Mcp.Tests/GroupDocs.Annotation.Mcp.Tests.csproj -c Release`
now reports **18/18 passed** locally on Windows. CI re-run should be green
across all three OSes once this is merged.
