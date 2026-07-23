using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

public class GeneratePagesPreviewTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GeneratePagesPreviewTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GeneratePagesPreview_BlankPdf_ReturnsImageContent()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.GeneratePagesPreview.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {body}");
        var hasImage = response.Content.Any(c => c is ModelContextProtocol.Protocol.ImageContentBlock);
        Assert.True(hasImage, $"Expected at least one ImageContentBlock in the response. Got: {string.Join(", ", response.Content.Select(c => c.GetType().Name))}");
    }

}
