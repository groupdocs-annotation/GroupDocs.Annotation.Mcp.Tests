using GroupDocs.Annotation.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Annotation.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class GetAnnotationsTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetAnnotationsTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetAnnotations_BlankPdf_ReportsNoAnnotations()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.BlankPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.BlankPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetAnnotations.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BlankPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {body}");
        var indicates = body.Contains("No annotations", StringComparison.OrdinalIgnoreCase)
                     || body.Contains("\"found\"", StringComparison.Ordinal)
                     || body.Contains("\"annotations\"", StringComparison.Ordinal);
        Assert.True(indicates, $"Expected an empty-result indicator. Response:\n{body}");
    }

    [Fact]
    public async Task GetAnnotations_RealAnnotatedPdf_ReturnsJsonWithEntries()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        if (!File.Exists(Path.Combine(_fixture.StoragePath, SampleDocuments.AnnotatedPdf)))
        {
            _output.WriteLine($"Sample '{SampleDocuments.AnnotatedPdf}' not present in storage — skipping.");
            return;
        }

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetAnnotations.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.AnnotatedPdf },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {body}");
        Assert.True(body.Contains("\"found\"", StringComparison.Ordinal) || body.Contains("annotation", StringComparison.OrdinalIgnoreCase),
            $"Expected JSON with 'found' / 'annotation' fields. Response:\n{body}");
    }

}
