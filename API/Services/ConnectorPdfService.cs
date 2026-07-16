using System.Text;
using API.Configuration;
using API.DTOs;
using API.Models;
using API.Repositories;
using Microsoft.Extensions.Options;

namespace API.Services;

public interface IConnectorPdfService
{
    Task<(bool Success, string? Error, byte[]? PdfBytes, string? FileName)> GenerateAsync(
        Guid projectId,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, byte[]? EmlBytes, string? FileName)> BuildOutlookDraftAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
}

public class ConnectorPdfService : IConnectorPdfService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IPdfGenerationService _pdfGeneration;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ConnectorPdfService> _logger;

    public ConnectorPdfService(
        IProjectRepository projectRepository,
        IAttachmentRepository attachmentRepository,
        IFileStorageService fileStorage,
        IPdfGenerationService pdfGeneration,
        IOptions<EmailSettings> emailSettings,
        ILogger<ConnectorPdfService> logger)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
        _pdfGeneration = pdfGeneration;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, byte[]? PdfBytes, string? FileName)> GenerateAsync(
        Guid projectId,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return (false, "Project not found.", null, null);

        if (formData is null || formData.Count == 0)
            return (false, "Form data is required to generate the PDF.", null, null);

        var normalized = NormalizeFormData(formData);
        var attachments = await _attachmentRepository.GetByProjectIdAsync(projectId, cancellationToken);
        var supporting = new List<AttachmentFileContent>();

        foreach (var attachment in attachments)
        {
            var path = _fileStorage.GetAbsolutePath(attachment.StoragePath);
            if (path is null) continue;
            supporting.Add(new AttachmentFileContent(attachment.FileName, attachment.ContentType, path));
        }

        try
        {
            var pdfBytes = _pdfGeneration.GenerateConnectorPdf(project, normalized, supporting);
            var fileName = BuildPdfFileName(project);
            await _fileStorage.SaveGeneratedPdfAsync(projectId, pdfBytes, fileName, cancellationToken);
            _logger.LogInformation("Generated connector PDF for project {ProjectId} ({Bytes} bytes)", projectId, pdfBytes.Length);
            return (true, null, pdfBytes, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed for project {ProjectId}", projectId);
            return (false, "PDF generation failed. Please try again.", null, null);
        }
    }

    public async Task<(bool Success, string? Error, byte[]? EmlBytes, string? FileName)> BuildOutlookDraftAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return (false, "Project not found.", null, null);

        var pdfPath = _fileStorage.GetLatestGeneratedPdfPath(projectId);
        if (pdfPath is null)
            return (false, "Generate the PDF before sharing via email.", null, null);

        var pdfBytes = await File.ReadAllBytesAsync(pdfPath, cancellationToken);
        var pdfFileName = _fileStorage.GetLatestGeneratedFileName(projectId)
            ?? BuildPdfFileName(project);

        var to = _emailSettings.ConnectorTeamAddress;
        var subject = $"Connector Requirement Gathering — {project.ClientName} / {project.ApplicationName}";
        var body =
            $"Dear Connector Team,{Environment.NewLine}{Environment.NewLine}" +
            $"Please find attached the Connector Information Gathering document for the following engagement:{Environment.NewLine}{Environment.NewLine}" +
            $"Client: {project.ClientName}{Environment.NewLine}" +
            $"Application: {project.ApplicationName}{Environment.NewLine}" +
            $"Project: {project.Name}{Environment.NewLine}{Environment.NewLine}" +
            $"Kindly review and proceed with connector development as applicable.{Environment.NewLine}{Environment.NewLine}" +
            $"Regards,{Environment.NewLine}" +
            $"{(string.IsNullOrWhiteSpace(project.ImplementationEngineer) ? project.CreatedBy : project.ImplementationEngineer)}{Environment.NewLine}" +
            $"ARCON — Implementation";

        var eml = BuildEml(to, subject, body, pdfFileName, pdfBytes);
        var emlName = $"Share_{Sanitize(project.ApplicationName)}.eml";
        return (true, null, eml, emlName);
    }

    private static Dictionary<string, string> NormalizeFormData(Dictionary<string, string> formData)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in formData)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            result[key] = value?.Trim() ?? string.Empty;
        }
        return result;
    }

    private static string BuildPdfFileName(Project project)
    {
        return $"CIGT_{Sanitize(project.ClientName)}_{Sanitize(project.ApplicationName)}.pdf";
    }

    private static string Sanitize(string value)
    {
        var cleaned = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Document" : cleaned.Replace(' ', '_');
    }

    private static byte[] BuildEml(string to, string subject, string body, string attachmentName, byte[] attachmentBytes)
    {
        var boundary = "----=_CIGT_" + Guid.NewGuid().ToString("N");
        var sb = new StringBuilder();
        sb.Append("To: ").Append(to).Append("\r\n");
        sb.Append("Subject: ").Append(EncodeHeader(subject)).Append("\r\n");
        sb.Append("X-Unsent: 1\r\n");
        sb.Append("MIME-Version: 1.0\r\n");
        sb.Append("Content-Type: multipart/mixed; boundary=\"").Append(boundary).Append("\"\r\n");
        sb.Append("\r\n");
        sb.Append("--").Append(boundary).Append("\r\n");
        sb.Append("Content-Type: text/plain; charset=\"utf-8\"\r\n");
        sb.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");
        sb.Append(body).Append("\r\n\r\n");
        sb.Append("--").Append(boundary).Append("\r\n");
        sb.Append("Content-Type: application/pdf; name=\"").Append(attachmentName).Append("\"\r\n");
        sb.Append("Content-Transfer-Encoding: base64\r\n");
        sb.Append("Content-Disposition: attachment; filename=\"").Append(attachmentName).Append("\"\r\n\r\n");
        sb.Append(ChunkBase64(Convert.ToBase64String(attachmentBytes)));
        sb.Append("\r\n--").Append(boundary).Append("--\r\n");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EncodeHeader(string value) =>
        value.Replace("\r", " ").Replace("\n", " ");

    private static string ChunkBase64(string base64)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < base64.Length; i += 76)
        {
            var len = Math.Min(76, base64.Length - i);
            sb.Append(base64, i, len).Append("\r\n");
        }
        return sb.ToString();
    }
}
