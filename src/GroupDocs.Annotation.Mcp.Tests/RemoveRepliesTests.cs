using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

public class RemoveRepliesTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RemoveRepliesTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task RemoveReplies_BlankPdf_ReportsNoReplies()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.RemoveReplies.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {body}");
        var indicates = body.Contains("No replies", StringComparison.OrdinalIgnoreCase)
                     || body.Contains("No annotations", StringComparison.OrdinalIgnoreCase);
        Assert.True(indicates, $"Expected an empty-result indicator. Response:\n{body}");
    }

}
