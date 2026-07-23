using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

public class UpdateAnnotationTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public UpdateAnnotationTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task UpdateAnnotation_MissingId_ReturnsDescriptiveHint()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.UpdateAnnotation.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["id"] = 999, ["message"] = "new",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("not found", body, StringComparison.OrdinalIgnoreCase);
    }

}
