# Use with Cursor

Connect the MCP server to [Cursor](https://cursor.com) so you can ask its Agent
to add, list, update, remove, import, export, or preview document annotations.

## Prerequisites

- Cursor installed and updated (MCP support is in **Settings → Tools & MCP**).
- One of:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for the `dnx` route — recommended), or
  - [Docker](https://www.docker.com/products/docker-desktop) (for the container route).

## Config file location

Cursor uses the **`mcpServers`** key (like Claude Desktop) — **not** `servers`
as in VS Code. Two scopes:

| Scope | Path |
|---|---|
| Global (all projects) | `~/.cursor/mcp.json` (macOS/Linux) · `%USERPROFILE%\.cursor\mcp.json` (Windows) |
| Project-only | `.cursor/mcp.json` in the workspace root |

Create the file if it doesn't exist.

## Option A — dnx (recommended)

```json
{
  "mcpServers": {
    "groupdocs-annotation": {
      "command": "dnx",
      "args": ["GroupDocs.Annotation.Mcp@26.7.0", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

- Replace the storage path with an **absolute path** to the folder Cursor should
  operate on. On Windows use `"C:\\Users\\you\\Documents"` (double-escaped) or
  forward slashes.
- Omit `@26.7.0` to always pull the latest stable.
- Add `"GROUPDOCS_LICENSE_PATH": "…/GroupDocs.Total.lic"` to `env` to remove the
  evaluation watermark from annotated output and previews.

Copy-paste starter: [examples/cursor-mcp.json](../examples/cursor-mcp.json).

## Option B — Windows: full path to `dotnet.exe` (SSL / timeout workaround)

On Windows, Cursor launching `dnx` can fail with an **SSL / ~30 s timeout** on
the first package probe. Bypass `dnx` by running the already-cached tool DLL
directly with `dotnet.exe`:

```json
{
  "mcpServers": {
    "groupdocs-annotation": {
      "command": "C:\\Program Files\\dotnet\\dotnet.exe",
      "args": [
        "C:\\Users\\you\\.nuget\\packages\\groupdocs.annotation.mcp\\26.7.0\\tools\\net10.0\\any\\GroupDocs.Annotation.Mcp.dll"
      ],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "C:\\Users\\you\\Documents"
      }
    }
  }
}
```

Populate the cache first by running `dnx GroupDocs.Annotation.Mcp@26.7.0 --yes` once
in a terminal, then point `args[0]` at the resulting
`…\.nuget\packages\groupdocs.annotation.mcp\<version>\tools\net10.0\any\GroupDocs.Annotation.Mcp.dll`.

## Option C — Docker

```json
{
  "mcpServers": {
    "groupdocs-annotation": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/you/Documents:/data",
        "ghcr.io/groupdocs-annotation/annotation-net-mcp:26.7.0"
      ]
    }
  }
}
```

The Docker image already bundles `libgdiplus`, `libfontconfig1`, and
`ttf-mscorefonts-installer`, so glyph rendering (annotation text baked onto pages,
`generate_pages_preview`) works out of the box. For the native `dnx` route on
Linux/macOS, install those first (see [01 — Install from NuGet](01-install-from-nuget.md)).

## Reload and verify

1. Save `mcp.json`.
2. **Settings → Tools & MCP** → find `groupdocs-annotation` → toggle it on (or hit
   the reload icon). A green dot means it connected.
3. Expand it — you should see `add_annotation`, `get_annotations`,
   `update_annotation`, `remove_annotations`, `add_reply`, `remove_replies`,
   `import_annotations`, `export_annotations`, `get_document_info`, and
   `generate_pages_preview`.

## Example prompts (Agent mode)

```
List every annotation in design-review.docx and group them by author.

Add a highlight on page 2 of contract.pdf around the indemnity clause with the comment "Needs legal review".

Reply to annotation 7 in spec.pdf with "Implemented in PR #42" under the user "alice".

Preview page 1 and page 5 of report.pdf with annotations baked in.
```

The Agent will call `get_annotations` / `add_annotation` / `add_reply` /
`generate_pages_preview` and compose its answer from the results.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Server greyed out / won't start on Windows | `dnx` SSL/timeout — use **Option B** (full `dotnet.exe` path + cached DLL). |
| Server not listed | JSON typo — Cursor silently drops unparseable entries. Validate with `jq . mcp.json`. Confirm the key is `mcpServers`, not `servers`. |
| Output has a watermark | Expected in evaluation mode. `get_annotations` / `get_document_info` are unaffected; add `GROUPDOCS_LICENSE_PATH` to remove watermarks from written documents and previews. |
| `DllNotFoundException: libgdiplus` (macOS/Linux) | Install native deps — `brew install mono-libgdiplus` (macOS) / `apt-get install libgdiplus libfontconfig1 ttf-mscorefonts-installer` (Linux), or use the Docker option. |

## Next steps

- [04 — Use with Claude Desktop](04-use-with-claude-desktop.md)
- [05 — Use with VS Code / Copilot](05-use-with-vscode-copilot.md)
