using ModelContextProtocol.Client;

namespace GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by keyword. The server-side attribute [McpServerTool] uses
/// the method name verbatim (PascalCase: AddAnnotation, GetAnnotations, …) and
/// the MCP SDK exposes the wire name in PascalCase OR snake_case depending on
/// SDK version — keywords used here work under both conventions.
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools) => _tools = tools;

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;

    // 'annotation' is unique to: AddAnnotation, GetAnnotations, UpdateAnnotation,
    // RemoveAnnotations, ImportAnnotations, ExportAnnotations. Disambiguate by
    // verb-substring + an excluding list when needed (e.g. AddAnnotation must
    // exclude 'reply' / 'preview' so it doesn't match AddReply / GeneratePagesPreview).
    public McpClientTool AddAnnotation        => Resolve("add",      excluding: new[] { "reply" });
    public McpClientTool GetAnnotations       => Resolve("get",      required: "annotation", excluding: new[] { "document" });
    public McpClientTool UpdateAnnotation     => Resolve("update");
    public McpClientTool RemoveAnnotations    => Resolve("remove",   required: "annotation");
    public McpClientTool AddReply             => Resolve("reply",    required: "add");
    public McpClientTool RemoveReplies        => Resolve("reply",    required: "remove");
    public McpClientTool ImportAnnotations    => Resolve("import");
    public McpClientTool ExportAnnotations    => Resolve("export");
    public McpClientTool GetDocumentInfo      => Resolve("document", required: "info");
    public McpClientTool GeneratePagesPreview => Resolve("preview");

    private McpClientTool Resolve(string keyword, string? required = null, string[]? excluding = null)
    {
        var ex = excluding ?? Array.Empty<string>();
        bool Matches(McpClientTool t)
        {
            var name = t.Name;
            if (!name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return false;
            if (required is not null && !name.Contains(required, StringComparison.OrdinalIgnoreCase))
                return false;
            foreach (var x in ex)
                if (name.Contains(x, StringComparison.OrdinalIgnoreCase))
                    return false;
            return true;
        }

        return _tools.FirstOrDefault(Matches)
            ?? throw new InvalidOperationException(
                $"No tool with name containing '{keyword}'{(required is not null ? $" and '{required}'" : "")}{(ex.Length > 0 ? $" but excluding [{string.Join(',', ex)}]" : "")}. " +
                $"Found: {string.Join(", ", _tools.Select(t => t.Name))}");
    }
}
