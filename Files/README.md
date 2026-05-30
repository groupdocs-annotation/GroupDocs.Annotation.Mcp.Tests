# Files/ — integration test samples

Real document samples used by the per-format `[Theory]` and per-tool tests.
The fixtures (`SampleDocuments.CopyRealSamples`) stage them into the
per-test temp storage folder; tests skip themselves when a sample is missing,
so this folder may be safely pruned.

## Provenance

All files are copied verbatim from the upstream
[GroupDocs.Annotation for .NET examples repo](https://github.com/groupdocs-annotation/GroupDocs.Annotation-for-.NET),
under `Examples/GroupDocs.Annotation.Examples.CSharp/Resources/SampleFiles/`.

| File                          | Source path                                              | Used by                                        |
|-------------------------------|----------------------------------------------------------|------------------------------------------------|
| `input.pdf`                   | `SampleFiles/input.pdf`                                  | AddAnnotation, GetDocumentInfo, GeneratePagesPreview |
| `input.docx`                  | `SampleFiles/input.docx`                                 | AddAnnotation, GetDocumentInfo                 |
| `input.xlsx`                  | `SampleFiles/input.xlsx`                                 | GetDocumentInfo                                |
| `annotated.pdf`                | `SampleFiles/annotated.pdf`                              | GetAnnotations, ExportAnnotations, UpdateAnnotation, RemoveAnnotations |
| `annotated.docx`               | `SampleFiles/annotated.docx`                             | GetAnnotations (DOCX path)                     |
| `annotated_with_replies.pdf`   | `SampleFiles/annotated_with_replies.pdf`                 | AddReply, RemoveReplies, GetAnnotations.replies |
| `annotation.xml`               | `SampleFiles/annotation.xml`                             | ImportAnnotations (XML route)                  |

## License

Examples-repo content is published under the MIT license (see the upstream
repo's LICENSE) — the same license as this Tests repo.

## Adding more

If you need a sample for a new format:

1. Pick the smallest representative file from the upstream examples repo
   (or generate a minimal valid file).
2. Drop it under `Files/`.
3. Add a `public const string Sample…` to
   `src/GroupDocs.Annotation.Mcp.Tests/Fixtures/SampleDocuments.cs` and
   include it in `RealSamples`.
4. Reference it from the relevant per-tool test class.
5. Update the provenance table above.
