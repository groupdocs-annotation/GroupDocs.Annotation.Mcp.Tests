# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

**Integration tests** for the [`GroupDocs.Annotation.Mcp`](https://www.nuget.org/packages/GroupDocs.Annotation.Mcp) NuGet package — an MCP server that exposes GroupDocs.Annotation for .NET as AI-callable tools.

This repo is **not** the server itself. The server lives at [groupdocs-annotation/GroupDocs.Annotation.Mcp](https://github.com/groupdocs-annotation/GroupDocs.Annotation.Mcp). This repo:

1. Consumes only the **published** NuGet artifact (no project references).
2. Launches the server via `dnx`, connects as an MCP stdio client, and exercises every advertised tool.
3. Doubles as a copy-pasteable set of example configs and how-to guides for all deployment channels (NuGet, Docker, MCP registry, Claude Desktop, VS Code, Cursor).

## Folder layout

```
src/GroupDocs.Annotation.Mcp.Tests/
  Fixtures/
    McpServerFixture.cs          ← launches dnx child process, wires stdio MCP client
    SampleDocuments.cs           ← builds minimal PDF + JPEG from byte arrays at runtime
    ToolCatalog.cs               ← keyword-based tool name resolution (annotations/replies/preview)
    ToolResponse.cs              ← CallToolResult text/JSON extraction
    CommandResolver.cs           ← cross-platform dnx.cmd resolution on Windows
    PackageVersion.cs            ← pulls version from env / assembly metadata / default
  ToolDiscoveryTests.cs          ← handshake, tools/list, schema validation (10 tools)
  AddAnnotationTests.cs          ← add a type + saved-output presence
  GetAnnotationsTests.cs         ← JSON shape (id / type / message / page / box / replies)
  UpdateAnnotationTests.cs       ← message / box edit by id
  RemoveAnnotationsTests.cs      ← by id list / all
  AddReplyTests.cs               ← reply thread by annotation id
  RemoveRepliesTests.cs          ← by reply-id list / user / all
  ImportAnnotationsTests.cs      ← XML / document source dispatch
  ExportAnnotationsTests.cs      ← XML dump presence
  GetDocumentInfoTests.cs        ← JSON shape + per-format theory
  GeneratePagesPreviewTests.cs   ← inline PNG image content blocks
  ErrorHandlingTests.cs          ← unknown file, corrupted bytes, password parameter
  GroupDocs.Annotation.Mcp.Tests.csproj
.github/workflows/integration.yml  ← matrix × 3 OS, nightly cron, release-smoke dispatch
changelog/                         ← one MD file per change (NNN-slug.md)
how-to/                            ← user-facing guides for every deployment channel
examples/                          ← claude-desktop.json, vscode-mcp.json, docker-compose.yml
Files/                       ← drop real fixture files here; copied to test output
Directory.Build.props              ← McpPackageVersion property (overridable)
global.json                        ← pinned to .NET 10.0.100
```

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts via `dnx` | `McpServerFixture` |
| MCP handshake, server info, 10-tool advertisement | `ToolDiscoveryTests` |
| `add_annotation` / `update_annotation` / `remove_annotations` | `AddAnnotationTests` / `UpdateAnnotationTests` / `RemoveAnnotationsTests` |
| `get_annotations` / `get_document_info` — JSON shape | `GetAnnotationsTests` / `GetDocumentInfoTests` |
| `add_reply` / `remove_replies` — comment threads | `AddReplyTests` / `RemoveRepliesTests` |
| `import_annotations` / `export_annotations` — XML round-trip | `ImportAnnotationsTests` / `ExportAnnotationsTests` |
| `generate_pages_preview` — inline PNG image blocks | `GeneratePagesPreviewTests` |
| Unknown / corrupted files, password parameter | `ErrorHandlingTests` |

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build -c Release

# Run all 12 tests against the default package version (26.7.0)
dotnet test -c Release

# Run against a specific published version
dotnet test -c Release -p:McpPackageVersion=26.7.0
# or
MCP_PACKAGE_VERSION=26.7.0 dotnet test -c Release

# Unlock licensed-mode Verify tests
GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic dotnet test -c Release

# Run just the discovery suite (fastest — no tool invocations)
dotnet test -c Release --filter "FullyQualifiedName~ToolDiscovery"
```

## Key design decisions

1. **Keyword-based tool resolution.** `ToolCatalog.Resolve("preview")` picks the tool whose snake_case wire name contains the keyword (case-insensitive). The MCP C# SDK converts `[McpServerTool]` method names to `snake_case` — so the actual wire names are `add_annotation`, `get_annotations`, `generate_pages_preview`, etc., not `AddAnnotation`. Keywords must be snake_case substrings of the wire name (Pitfall #15). Tests stay robust if that convention changes.

2. **Fixtures.** `SampleDocuments.cs` builds a minimal blank PDF and a baseline 1×1 JPEG from byte arrays at startup. Real PDF / DOCX / XLSX / JPG / PNG samples — copied verbatim from the upstream `GroupDocs.Annotation-for-.NET` examples repo — live under `Files/` and the csproj auto-copies them into the test output (see `Files/README.md` for provenance). Per-format theories skip themselves when a sample is missing.

3. **Evaluation mode.** All 10 tools function without a license. The document-writing tools (`add_annotation`, `update_annotation`, `remove_annotations`, `add_reply`, `remove_replies`, `import_annotations`) and `generate_pages_preview` may emit watermarked output and prefix their response with `"[Evaluation mode]"`; the read-only tools (`get_annotations`, `get_document_info`) are unaffected. To exercise the watermark-free path, set `GROUPDOCS_LICENSE_PATH` — CI auto-decodes a `GROUPDOCS_LICENSE` repo secret into `$RUNNER_TEMP`.

4. **JSON responses are raw.** `get_annotations` and `get_document_info` serialise JSON directly (Pitfall #16 — never via `OutputHelper.TruncateText`), so tests safely `JsonDocument.Parse` the response body.

5. **No project references to the server.** The csproj only references `ModelContextProtocol` 1.1.0. If the server source breaks in the sibling repo, these tests still pass — they validate the shipped NuGet artifact.

## House rules

1. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md` (schema in `changelog/README.md`).
2. **How-to guides track deployment reality** — if the main repo publishes a new channel (e.g. new Docker registry), add a guide under `how-to/` *and* update `README.md`.
3. **Version bumps flow through `Directory.Build.props`** — `<McpPackageVersion>` is the single source of truth for "what version are we testing." CI overrides it via env var / workflow input.
4. **Tests must not require the main repo's source.** If a test needs a server-side change, file an issue there — don't work around it here.
5. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release smoke hook

The main repo's `publish_prod.yml` should fire a `repository_dispatch` with `event_type=nuget-published` after `dotnet nuget push` succeeds. The workflow in `.github/workflows/integration.yml` consumes `client_payload.package_version` and runs the matrix against the just-published version. This closes the loop: publish → smoke-test live nuget.org → fail loud if broken.

## What NOT to change

- Do not add a `ProjectReference` to the main repo's `GroupDocs.Annotation.Mcp.csproj`. This repo exists to test the shipped NuGet, not the source.
- Do not hardcode tool names as string literals (`"add_annotation"`). Use the `ToolCatalog` accessors (`ToolCatalog.AddAnnotation.Name`, etc.).
- Do not commit real license files or binary fixtures with unclear provenance. License goes through the `GROUPDOCS_LICENSE` CI secret; fixtures in `Files/` must be self-authored or CC0/Apache-2.0.
