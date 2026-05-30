---
id: 001
date: 2026-05-29
package-under-test: 26.5.0
type: feature
---

# Initial integration test suite for GroupDocs.Annotation.Mcp

## What changed

- xUnit test project targeting `net10.0`, referencing only the published
  `ModelContextProtocol` 1.1.0 NuGet — no project reference to the server source.
- `McpServerFixture` launches the published `GroupDocs.Annotation.Mcp@26.5.0`
  package via `dnx` as a child process, wires an MCP stdio client, and seeds a
  temporary storage folder with synthetic + real sample documents.
- `SampleDocuments` builds a minimal blank PDF and a baseline 1×1 JPEG from
  byte arrays at runtime, and copies real samples from `Files/` (committed
  `input.pdf` / `input.docx` / `input.xlsx` / `annotated.pdf` / `annotated.docx`
  / `annotated_with_replies.pdf` / `annotation.xml` from the upstream
  GroupDocs.Annotation-for-.NET examples repo — see [Files/README.md](../Files/README.md)
  for provenance).
- Twelve test classes covering all 10 tools advertised by the server:
  - `ToolDiscoveryTests` (3) — server info, exactly-10-tools, schema validation.
  - `AddAnnotationTests` (2) — textfield happy-path on blank PDF; unknown-type
    error path (Pitfall #18 `Annotation failed for` prefix).
  - `GetAnnotationsTests` (2) — empty-result on blank PDF; JSON-with-entries
    on real annotated.pdf.
  - `UpdateAnnotationTests` (1) — missing-id descriptive hint.
  - `RemoveAnnotationsTests` (1) — empty-result on blank PDF.
  - `AddReplyTests` (1) — missing annotation id descriptive hint.
  - `RemoveRepliesTests` (1) — empty-result on blank PDF.
  - `ImportAnnotationsTests` (1) — missing-source response shape.
  - `ExportAnnotationsTests` (1) — empty-result on blank PDF.
  - `GetDocumentInfoTests` (1) — raw JSON object shape on blank PDF.
  - `GeneratePagesPreviewTests` (1) — asserts the response contains an
    `ImageContentBlock` (Pitfall #18 in CallToolResult form when the engine
    fails).
  - `ErrorHandlingTests` (3) — unknown file, corrupted bytes, password parameter.
- GitHub Actions workflow `.github/workflows/integration.yml`:
  - Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`.
  - Linux step installs `libgdiplus` + `libfontconfig1` + `ttf-mscorefonts-installer`
    (with debconf EULA accept + `fc-cache`) because the engine rasterises
    annotated pages and needs real fonts for the message-glyph rendering.
  - macOS step `brew install mono-libgdiplus` and copies `libgdiplus.dylib`
    into the .NET shared-framework directory so dnx's child process can
    `dlopen` it.
  - Triggers: push, PR, nightly cron, `workflow_dispatch` (with `package_version`
    input), `repository_dispatch` (`nuget-published` event for release smoke).
  - Optional `GROUPDOCS_LICENSE` repo secret auto-decoded into `$RUNNER_TEMP`
    and exported as `GROUPDOCS_LICENSE_PATH` to drop the eval-mode watermark
    on add/update/remove paths.
- `examples/` — ready-to-use `claude-desktop.json`, `vscode-mcp.json`,
  `docker-compose.yml` copy-paste configs.
- `AGENTS.md` + `llms.txt` for AI coding agent orientation.
- `how-to/` guides covering every deployment channel (NuGet via dnx / dotnet
  tool, Docker, MCP registry, Claude Desktop, VS Code / GitHub Copilot, plus
  running this test suite).

## Why

Closes the release-validation gap: the main repo's unit tests mock
`IFileResolver` / `ILicenseManager` and validate tool logic, but nothing
previously exercised the **shipped** NuGet end-to-end. Every release now has
a cross-platform smoke check against live nuget.org before users hit it.

## Migration / impact

First release of this repository — no migration. To wire the release-smoke
trigger, add a `gh api repos/.../dispatches -f event_type=nuget-published -f
'client_payload[package_version]=…'` step to the main repo's publish workflow
after `dotnet nuget push` succeeds. See `how-to/06-run-integration-tests.md`.
