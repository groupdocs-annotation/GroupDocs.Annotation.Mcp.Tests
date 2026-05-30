using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class AddReplyTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public AddReplyTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task AddReply_MissingAnnotation_ReturnsDescriptiveHint()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.AddReply.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["annotationId"] = 999, ["comment"] = "looks ok", ["userName"] = "tester",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("not found", body, StringComparison.OrdinalIgnoreCase);
    }

}
