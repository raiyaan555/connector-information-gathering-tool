using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using API.DTOs;
using API.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace API.Services;

public interface IPdfGenerationService
{
    byte[] GenerateConnectorPdf(Project project, Dictionary<string, string> formData, IReadOnlyList<AttachmentFileContent> supportingFiles);
}

public record AttachmentFileContent(
    string FileName,
    string ContentType,
    string AbsolutePath);

public class PdfGenerationService : IPdfGenerationService
{
    // ARCON brand red (matches logo triangle)
    private static readonly string BrandRed = "#E30613";
    private static readonly string BrandRedDark = "#B80510";

    private readonly string _logoPath;

    static PdfGenerationService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public PdfGenerationService(IWebHostEnvironment env)
    {
        var candidates = new[]
        {
            Path.Combine(env.ContentRootPath, "Assets", "arcon-logo.png"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "arcon-logo.png"),
        };
        _logoPath = candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    public byte[] GenerateConnectorPdf(
        Project project,
        Dictionary<string, string> formData,
        IReadOnlyList<AttachmentFileContent> supportingFiles)
    {
        var contentPdf = BuildFormContentPdf(project, formData, supportingFiles);
        return MergeWithSupportingFiles(contentPdf, supportingFiles);
    }

    private byte[] BuildFormContentPdf(Project project, Dictionary<string, string> formData, IReadOnlyList<AttachmentFileContent> supportingFiles)
    {
        var sections = FormFieldCatalog.BuildSections(formData);
        var generatedAt = DateTime.Now.ToString("dd MMM yyyy HH:mm");
        var hasLogo = File.Exists(_logoPath);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Row(brand =>
                        {
                            if (hasLogo)
                            {
                                brand.ConstantItem(110).Height(40).Image(_logoPath).FitArea();
                                brand.ConstantItem(12);
                            }

                            brand.RelativeItem().AlignMiddle().Column(c =>
                            {
                                if (!hasLogo)
                                    c.Item().Text("ARCON").Bold().FontSize(14).FontColor(BrandRed);
                                c.Item().Text("Connector Information Gathering Tool (CIGT)")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                        row.ConstantItem(160).AlignRight().AlignMiddle().Column(c =>
                        {
                            c.Item().Text("CONFIDENTIAL").Bold().FontSize(9).FontColor(BrandRed);
                            c.Item().Text($"Generated: {generatedAt}").FontSize(8);
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(BrandRed);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Text("Connector Requirement Gathering Document")
                        .Bold().FontSize(18).FontColor(BrandRedDark);

                    col.Item().Border(1).BorderColor(BrandRed).Background(Colors.Grey.Lighten4).Padding(12).Column(meta =>
                    {
                        meta.Spacing(4);
                        meta.Item().Text(t =>
                        {
                            t.Span("Project: ").Bold().FontColor(BrandRedDark);
                            t.Span(project.Name);
                        });
                        meta.Item().Text(t =>
                        {
                            t.Span("Client: ").Bold().FontColor(BrandRedDark);
                            t.Span(project.ClientName);
                        });
                        meta.Item().Text(t =>
                        {
                            t.Span("Application: ").Bold().FontColor(BrandRedDark);
                            t.Span(project.ApplicationName);
                        });
                        meta.Item().Text(t =>
                        {
                            t.Span("Prepared by: ").Bold().FontColor(BrandRedDark);
                            t.Span(string.IsNullOrWhiteSpace(project.ImplementationEngineer)
                                ? project.CreatedBy
                                : project.ImplementationEngineer!);
                        });
                    });

                    col.Item().Text(
                            "This document consolidates the Connector Information Gathering responses for review by the Connector Team.")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);

                    foreach (var section in sections)
                    {
                        if (section.Fields.Count == 0) continue;

                        col.Item().PaddingTop(6).Column(sectionCol =>
                        {
                            sectionCol.Item()
                                .Background(BrandRed)
                                .Padding(8)
                                .Text(section.Title)
                                .Bold()
                                .FontColor(Colors.White)
                                .FontSize(11);

                            sectionCol.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Column(fields =>
                            {
                                for (var i = 0; i < section.Fields.Count; i++)
                                {
                                    var field = section.Fields[i];
                                    var bg = i % 2 == 0 ? "#FFFFFF" : "#FFF5F5";
                                    fields.Item().Background(bg).Padding(8).Column(f =>
                                    {
                                        f.Item().Text(field.Label).Bold().FontSize(9).FontColor(BrandRedDark);
                                        f.Item().PaddingTop(2).Text(string.IsNullOrWhiteSpace(field.Value) ? "—" : field.Value)
                                            .FontSize(10);
                                    });
                                }
                            });
                        });
                    }

                    var pdfAttachments = supportingFiles
                        .Where(f => IsPdf(f) || IsImage(f))
                        .Select(f => f.FileName)
                        .ToList();

                    if (pdfAttachments.Count > 0)
                    {
                        col.Item().PaddingTop(8).Column(att =>
                        {
                            att.Item()
                                .Background(BrandRed)
                                .Padding(8)
                                .Text("Supporting Attachments (appended)")
                                .Bold()
                                .FontColor(Colors.White)
                                .FontSize(11);
                            att.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(list =>
                            {
                                foreach (var name in pdfAttachments)
                                    list.Item().Text($"• {name}").FontSize(10);
                            });
                        });
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("ARCON CIGT · Page ").FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontColor(BrandRed);
                    t.Span(" of ").FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontColor(BrandRed);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static byte[] MergeWithSupportingFiles(byte[] contentPdf, IReadOnlyList<AttachmentFileContent> files)
    {
        using var output = new PdfDocument();
        AppendPdfBytes(output, contentPdf);

        foreach (var file in files)
        {
            if (!File.Exists(file.AbsolutePath))
                continue;

            if (IsPdf(file))
            {
                try
                {
                    using var input = PdfReader.Open(file.AbsolutePath, PdfDocumentOpenMode.Import);
                    for (var i = 0; i < input.PageCount; i++)
                        output.AddPage(input.Pages[i]);
                }
                catch
                {
                    // Skip unreadable PDFs rather than failing the whole generation.
                }
            }
            else if (IsImage(file))
            {
                try
                {
                    AppendImagePage(output, file.AbsolutePath, file.FileName);
                }
                catch
                {
                    // Skip bad images.
                }
            }
        }

        using var ms = new MemoryStream();
        output.Save(ms, false);
        return ms.ToArray();
    }

    private static void AppendPdfBytes(PdfDocument output, byte[] pdfBytes)
    {
        using var ms = new MemoryStream(pdfBytes);
        using var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import);
        for (var i = 0; i < input.PageCount; i++)
            output.AddPage(input.Pages[i]);
    }

    private static void AppendImagePage(PdfDocument output, string imagePath, string title)
    {
        var page = output.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        using var gfx = XGraphics.FromPdfPage(page);

        var margin = 40.0;
        var maxWidth = page.Width - (margin * 2);
        var maxHeight = page.Height - (margin * 2) - 30;

        gfx.DrawString(
            title,
            new XFont("Arial", 11, XFontStyle.Bold),
            new XSolidBrush(XColor.FromArgb(0xE3, 0x06, 0x13)),
            new XRect(margin, 20, maxWidth, 20),
            XStringFormats.TopLeft);

        using var image = XImage.FromFile(imagePath);
        var imgW = image.PixelWidth;
        var imgH = image.PixelHeight;
        var scale = Math.Min(maxWidth / imgW, maxHeight / imgH);
        var drawW = imgW * scale;
        var drawH = imgH * scale;
        var x = margin + ((maxWidth - drawW) / 2);
        var y = 50 + ((maxHeight - drawH) / 2);

        gfx.DrawImage(image, x, y, drawW, drawH);
    }

    private static bool IsPdf(AttachmentFileContent file)
    {
        var n = file.FileName.ToLowerInvariant();
        var t = file.ContentType.ToLowerInvariant();
        return t.Contains("pdf") || n.EndsWith(".pdf");
    }

    private static bool IsImage(AttachmentFileContent file)
    {
        var n = file.FileName.ToLowerInvariant();
        var t = file.ContentType.ToLowerInvariant();
        return t.Contains("png") || t.Contains("jpeg") || t.Contains("jpg")
               || n.EndsWith(".png") || n.EndsWith(".jpg") || n.EndsWith(".jpeg");
    }
}

internal static class FormFieldCatalog
{
    private static readonly (string Title, string[] Keys)[] SectionDefs =
    [
        ("About Application", [
            "applicationPurpose", "isSourceOfTruth", "hasUatEnvironment", "uatServer", "uatUsername",
            "uatPassword", "applicationType", "connectionMethod", "isLegacyApplication", "legacyDetails"
        ]),
        ("Application Integration", [
            "lifecycleFeatures", "userOnboardingRequired", "userOnboardingDetails", "userModificationRequired",
            "userModificationDetails", "userDeletionRequired", "userDeletionDetails", "deleteType",
            "userReactivationRequired", "reactivationMethod", "ssoRequired", "ssoType", "reconStrategy",
            "defaultEntitlement", "reconUserTypes", "entitlementTypes"
        ]),
        ("Converged Identity", ["ciPackage", "ciIntegrationRole", "moduleDiagramNotes"]),
        ("Source Of Truth", ["sotOnboardingStrategy", "onboardingScan", "sotAttributes", "additionalSotAttributes"]),
        ("Encryption", ["encryptedFields", "apiPayloadEncrypted", "encodedFields", "encryptionAlgorithm"]),
        ("General Information", ["apiDocumentationLink"]),
        ("Special Comments", ["specialComments"]),
    ];

    private static readonly Dictionary<string, string> Labels = new()
    {
        ["applicationPurpose"] = "What does this application do?",
        ["isSourceOfTruth"] = "Will this application be the Source of Truth (SOT)?",
        ["hasUatEnvironment"] = "Do we have the UAT environment to build/test/freeze the connectors?",
        ["uatServer"] = "UAT Server",
        ["uatUsername"] = "UAT Username",
        ["uatPassword"] = "UAT Password",
        ["applicationType"] = "What type of application is it?",
        ["connectionMethod"] = "How do we connect to this application?",
        ["isLegacyApplication"] = "Is this a legacy application or web application?",
        ["legacyDetails"] = "Legacy / Client Details",
        ["lifecycleFeatures"] = "Which lifecycle management features are required?",
        ["userOnboardingRequired"] = "Is user required to be on-boarded on the application?",
        ["userOnboardingDetails"] = "On-boarding Details",
        ["userModificationRequired"] = "Is user required to be modified on the application?",
        ["userModificationDetails"] = "Modification Details",
        ["userDeletionRequired"] = "Is user deletion required on the application?",
        ["userDeletionDetails"] = "Deletion Details",
        ["deleteType"] = "Is the removal of user a soft delete or hard delete?",
        ["userReactivationRequired"] = "Is the user required to be reactivated?",
        ["reactivationMethod"] = "Reactivation Method",
        ["ssoRequired"] = "Will there be SSO?",
        ["ssoType"] = "What type of SSO will be used for this application?",
        ["reconStrategy"] = "What is the recon strategy that will be used for this application?",
        ["defaultEntitlement"] = "While creating a user, does that user need to be assigned to some default entitlement?",
        ["reconUserTypes"] = "While reconciliation are the active users & the disable users coming in the same request?",
        ["entitlementTypes"] = "Will the user be assigned to multiple types of entitlements or only one type?",
        ["ciPackage"] = "Which CI Package will be getting implemented?",
        ["ciIntegrationRole"] = "How will it relate to the CI once integrated?",
        ["moduleDiagramNotes"] = "Module Diagram of integration of this application with CI",
        ["sotOnboardingStrategy"] = "What is the SOT on-boarding strategy that will be used?",
        ["onboardingScan"] = "What is the on-boarding scan that will be configured?",
        ["sotAttributes"] = "What are the attributes of the SOT that will be used for this application?",
        ["additionalSotAttributes"] = "Additional SOT Attributes",
        ["encryptedFields"] = "Which fields of the user details are encrypted?",
        ["apiPayloadEncrypted"] = "Are the api payloads encrypted?",
        ["encodedFields"] = "Which fields are encoded?",
        ["encryptionAlgorithm"] = "Is there any specific standard encryption algorithm used?",
        ["apiDocumentationLink"] = "Attach the api documentation for the collection",
        ["specialComments"] = "Special Comments (If Any)",
    };

    public static List<PdfSectionDto> BuildSections(Dictionary<string, string> formData)
    {
        var result = new List<PdfSectionDto>();
        foreach (var (title, keys) in SectionDefs)
        {
            var fields = new List<PdfFieldDto>();
            foreach (var key in keys)
            {
                if (!formData.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                    continue;
                fields.Add(new PdfFieldDto
                {
                    Label = Labels.GetValueOrDefault(key, key),
                    Value = value.Trim()
                });
            }
            result.Add(new PdfSectionDto { Title = title, Fields = fields });
        }
        return result;
    }
}
