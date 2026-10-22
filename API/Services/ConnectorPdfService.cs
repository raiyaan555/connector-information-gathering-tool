using System.IO.Compression;
using API.Configuration;
using API.DTOs;
using API.Helpers;
using API.Models;
using API.Repositories;
using Microsoft.Extensions.Options;

namespace API.Services;

public interface IConnectorPdfService
{
    Task<(bool Success, string? Error, byte[]? FileBytes, string? FileName, string? ContentType)> GenerateAsync(
        Guid projectId,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, ShareComposeResponse? Compose)> BuildShareComposeAsync(
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
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ConnectorPdfService> _logger;

    public ConnectorPdfService(
        IProjectRepository projectRepository,
        IAttachmentRepository attachmentRepository,
        IFileStorageService fileStorage,
        IPdfGenerationService pdfGeneration,
        IOptions<EmailSettings> emailSettings,
        ICurrentUser currentUser,
        ILogger<ConnectorPdfService> logger)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
        _pdfGeneration = pdfGeneration;
        _emailSettings = emailSettings.Value;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, byte[]? FileBytes, string? FileName, string? ContentType)> GenerateAsync(
        Guid projectId,
        Dictionary<string, string> formData,
        CancellationToken cancellationToken = default)
    {
        var project = await OwnedProjectAsync(projectId, cancellationToken);
        if (project is null)
            return (false, "Project not found.", null, null, null);

        if (formData is null || formData.Count == 0)
            return (false, "Form data is required to generate the PDF.", null, null, null);

        var normalized = NormalizeFormData(formData);
        var supporting = await LoadSupportingFilesAsync(projectId, cancellationToken);

        try
        {
            var pdfBytes = _pdfGeneration.GenerateConnectorPdf(project, normalized, supporting);
            var pdfFileName = BuildPdfFileName(project);
            await _fileStorage.SaveGeneratedPdfAsync(projectId, pdfBytes, pdfFileName, cancellationToken);

            var jsonFiles = supporting.Where(AttachmentKinds.IsJson).ToList();
            if (jsonFiles.Count > 0)
            {
                var zipName = BuildZipFileName(project);
                var zipBytes = BuildZip(pdfFileName, pdfBytes, jsonFiles);
                _logger.LogInformation(
                    "Generated connector PDF and JSON zip for project {ProjectId} ({Bytes} bytes)",
                    projectId,
                    zipBytes.Length);
                return (true, null, zipBytes, zipName, "application/zip");
            }

            _logger.LogInformation("Generated connector PDF for project {ProjectId} ({Bytes} bytes)", projectId, pdfBytes.Length);
            return (true, null, pdfBytes, pdfFileName, "application/pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed for project {ProjectId}", projectId);
            return (false, "PDF generation failed. Please try again.", null, null, null);
        }
    }

    public async Task<(bool Success, string? Error, ShareComposeResponse? Compose)> BuildShareComposeAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await OwnedProjectAsync(projectId, cancellationToken);
        if (project is null)
            return (false, "Project not found.", null);

        var to = _emailSettings.ConnectorTeamAddress?.Trim();
        if (string.IsNullOrWhiteSpace(to))
            return (false, "The connector team address is not configured.", null);

        var jsonFiles = (await LoadSupportingFilesAsync(projectId, cancellationToken))
            .Where(AttachmentKinds.IsJson)
            .ToList();
        var hasJson = jsonFiles.Count > 0;
        var attachmentName = hasJson ? BuildZipFileName(project) : BuildPdfFileName(project);
        var teamName = string.IsNullOrWhiteSpace(_emailSettings.ConnectorTeamName)
            ? "CI Connectors Team"
            : _emailSettings.ConnectorTeamName.Trim();
        var engineer = string.IsNullOrWhiteSpace(project.ImplementationEngineer)
            ? project.CreatedBy
            : project.ImplementationEngineer;
        var jiraKey = JiraTicketRules.IssueKeyOrNull(project.JiraTicket);
        var subject = $"Connector Requirement Gathering — {project.ClientName} / {project.ApplicationName}";
        if (!string.IsNullOrWhiteSpace(jiraKey))
            subject += $" — {jiraKey}";
        subject = subject.Replace("\r", " ").Replace("\n", " ");

        var jiraLine = string.IsNullOrWhiteSpace(project.JiraTicket)
            ? string.Empty
            : $"Jira ticket: {project.JiraTicket.Trim()}\n";
        var attachmentNote = hasJson
            ? $"The attached file is a zip named {attachmentName}. It contains the PDF and the JSON files for this project.\n\n"
            : $"The attached file is {attachmentName}.\n\n";
        var body =
            $"Dear {teamName},\n\n" +
            "Please find attached the Connector Information Gathering document for the following engagement:\n\n" +
            $"Client: {project.ClientName}\n" +
            $"Application: {project.ApplicationName}\n" +
            $"Project: {project.Name}\n" +
            jiraLine +
            "\n" +
            attachmentNote +
            "Kindly review and proceed with connector development as applicable.\n\n" +
            "Regards,\n" +
            $"{engineer}\n" +
            "ARCON — Implementation";

        return (true, null, new ShareComposeResponse
        {
            To = to,
            ToName = teamName,
            Subject = subject,
            Body = body,
            AttachmentFileName = attachmentName
        });
    }

    private async Task<Project?> OwnedProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return null;

        return await _projectRepository.GetByIdAsync(projectId, ownerId, cancellationToken);
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

    private async Task<List<AttachmentFileContent>> LoadSupportingFilesAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var attachments = await _attachmentRepository.GetByProjectIdAsync(projectId, cancellationToken);
        var supporting = new List<AttachmentFileContent>();
        foreach (var attachment in attachments)
        {
            var path = _fileStorage.GetAbsolutePath(attachment.StoragePath);
            if (path is null) continue;
            supporting.Add(new AttachmentFileContent(attachment.FileName, attachment.ContentType, path));
        }

        return supporting;
    }

    private static string BuildDownloadBaseName(Project project) =>
        $"CIGT_{Sanitize(project.ClientName)}_{Sanitize(project.ApplicationName)}";

    private static string BuildPdfFileName(Project project) => $"{BuildDownloadBaseName(project)}.pdf";

    private static string BuildZipFileName(Project project) => $"{BuildDownloadBaseName(project)}.zip";

    private static byte[] BuildZip(string pdfFileName, byte[] pdfBytes, IReadOnlyList<AttachmentFileContent> jsonFiles)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            WriteZipEntry(archive, UniqueZipName(pdfFileName, used), pdfBytes);

            foreach (var json in jsonFiles)
            {
                if (!File.Exists(json.AbsolutePath))
                    continue;
                var entryName = UniqueZipName(Path.GetFileName(json.FileName), used);
                WriteZipEntry(archive, entryName, File.ReadAllBytes(json.AbsolutePath));
            }
        }

        return ms.ToArray();
    }

    private static void WriteZipEntry(ZipArchive archive, string name, byte[] bytes)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(bytes, 0, bytes.Length);
    }

    private static string UniqueZipName(string fileName, HashSet<string> used)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
            name = "file";
        name = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(name))
            name = "file";

        if (used.Add(name))
            return name;

        var stem = Path.GetFileNameWithoutExtension(name);
        var extension = Path.GetExtension(name);
        var index = 2;
        string candidate;
        do
        {
            candidate = $"{stem}_{index}{extension}";
            index++;
        }
        while (!used.Add(candidate));

        return candidate;
    }

    private static string Sanitize(string value)
    {
        var cleaned = string.Join("_", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Document" : cleaned.Replace(' ', '_');
    }
}
