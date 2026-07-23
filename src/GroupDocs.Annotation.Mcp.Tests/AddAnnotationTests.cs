using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

public class AddAnnotationTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public AddAnnotationTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task AddAnnotation_TextField_BlankPdf_WritesAnnotatedOutput()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.AddAnnotation.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = "textfield", ["text"] = "Hello", ["page"] = 1, ["x"] = 100, ["y"] = 100,
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {body}");
        var expected = Path.Combine(_fixture.StoragePath, "blank_annotated.pdf");
        Assert.True(File.Exists(expected),
            $"Expected output file at '{expected}'. Response:\n{body}");
    }

    [Fact]
    public async Task AddAnnotation_UnknownType_ReturnsDescriptiveError()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.AddAnnotation.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
                ["type"] = "not-a-real-type", ["text"] = "x",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("Annotation failed for", body, StringComparison.Ordinal);
    }

}
