using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ResumeAI.Export.Models;

namespace ResumeAI.Export.Services;

public interface IDocxRenderer
{
    byte[] Render(ResumeData resume);
}

public class OpenXmlDocxRenderer : IDocxRenderer
{
    public byte[] Render(ResumeData resume)
    {
        using var stream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Add page margins
            var sectionProps = new SectionProperties();
            sectionProps.AppendChild(new PageMargin
            {
                Top    = 720,
                Bottom = 720,
                Left   = 1080,
                Right  = 1080
            });
            body.AppendChild(sectionProps);

            // Name heading
            body.AppendChild(CreateHeading(resume.FullName, 28, bold: true));

            // Job title
            if (!string.IsNullOrWhiteSpace(resume.TargetJobTitle))
                body.AppendChild(CreateParagraph(resume.TargetJobTitle, 12, italic: true));

            // Contact info
            var contactParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(resume.Email))    contactParts.Add(resume.Email);
            if (!string.IsNullOrWhiteSpace(resume.Phone))    contactParts.Add(resume.Phone);
            if (!string.IsNullOrWhiteSpace(resume.Location)) contactParts.Add(resume.Location);
            if (contactParts.Count > 0)
                body.AppendChild(CreateParagraph(string.Join("  |  ", contactParts), 9));

            // Horizontal rule (bottom border on paragraph)
            body.AppendChild(CreateHorizontalRule());

            // Summary
            if (!string.IsNullOrWhiteSpace(resume.Summary))
            {
                body.AppendChild(CreateSectionHeading("Professional Summary"));
                body.AppendChild(CreateParagraph(resume.Summary, 10));
            }

            // Resume sections
            var visibleSections = resume.Sections
                .Where(s => s.IsVisible && !string.IsNullOrWhiteSpace(s.Content))
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            foreach (var section in visibleSections)
            {
                body.AppendChild(CreateSectionHeading(section.Title));
                body.AppendChild(CreateParagraph(section.Content!, 10));
            }
        }

        return stream.ToArray();
    }

    private static Paragraph CreateHeading(string text, int fontSize, bool bold = false)
    {
        var para = new Paragraph();
        var run  = new Run();

        var props = new RunProperties();
        props.AppendChild(new FontSize { Val = (fontSize * 2).ToString() });
        if (bold) props.AppendChild(new Bold());

        run.AppendChild(props);
        run.AppendChild(new Text(text));
        para.AppendChild(run);
        return para;
    }

    private static Paragraph CreateParagraph(string text, int fontSize, bool italic = false)
    {
        var para = new Paragraph();
        var run  = new Run();

        var props = new RunProperties();
        props.AppendChild(new FontSize { Val = (fontSize * 2).ToString() });
        if (italic) props.AppendChild(new Italic());
        props.AppendChild(new RunFonts { Ascii = "Calibri" });

        run.AppendChild(props);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        para.AppendChild(run);
        return para;
    }

    private static Paragraph CreateSectionHeading(string title)
    {
        var para      = new Paragraph();
        var paraProps = new ParagraphProperties();
        var border    = new ParagraphBorders();
        border.AppendChild(new BottomBorder
        {
            Val   = BorderValues.Single,
            Size  = 6,
            Space = 1,
            Color = "999999"
        });
        paraProps.AppendChild(new SpacingBetweenLines { Before = "120", After = "60" });
        paraProps.AppendChild(border);
        para.AppendChild(paraProps);

        var run  = new Run();
        var props = new RunProperties();
        props.AppendChild(new Bold());
        props.AppendChild(new FontSize { Val = "22" });
        props.AppendChild(new Color { Val = "444444" });
        run.AppendChild(props);
        run.AppendChild(new Text(title.ToUpper()));
        para.AppendChild(run);
        return para;
    }

    private static Paragraph CreateHorizontalRule()
    {
        var para      = new Paragraph();
        var paraProps = new ParagraphProperties();
        var border    = new ParagraphBorders();
        border.AppendChild(new BottomBorder
        {
            Val   = BorderValues.Single,
            Size  = 12,
            Space = 1,
            Color = "222222"
        });
        paraProps.AppendChild(border);
        paraProps.AppendChild(new SpacingBetweenLines { After = "120" });
        para.AppendChild(paraProps);
        return para;
    }
}
