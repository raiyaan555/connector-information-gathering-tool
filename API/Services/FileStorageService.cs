using API.Configuration;
using Microsoft.Extensions.Options;

namespace API.Services;

public interface IFileStorageService
{
    Task<string> SaveAttachmentAsync(Guid projectId, Guid attachmentId, string fileName, Stream content, CancellationToken cancellationToken = default);
    string? GetAbsolutePath(string? relativePath);
    Task<string> SaveGeneratedPdfAsync(Guid projectId, byte[] pdfBytes, string fileName, CancellationToken cancellationToken = default);
    string? GetLatestGeneratedPdfPath(Guid projectId);
    string? GetLatestGeneratedFileName(Guid projectId);
    void DeleteIfExists(string? relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _root;

    public FileStorageService(IOptions<StorageSettings> options, IWebHostEnvironment env)
    {
        var configured = options.Value.RootPath;
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAttachmentAsync(
        Guid projectId,
        Guid attachmentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeName = SanitizeFileName(fileName);
        var relativeDir = Path.Combine("attachments", projectId.ToString("N"));
        var absoluteDir = Path.Combine(_root, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var relativePath = Path.Combine(relativeDir, $"{attachmentId:N}_{safeName}");
        var absolutePath = Path.Combine(_root, relativePath);

        await using var fs = File.Create(absolutePath);
        await content.CopyToAsync(fs, cancellationToken);

        return relativePath.Replace('\\', '/');
    }

    public string? GetAbsolutePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var absolute = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(absolute) ? absolute : null;
    }

    public async Task<string> SaveGeneratedPdfAsync(
        Guid projectId,
        byte[] pdfBytes,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var safeName = SanitizeFileName(fileName);
        var relativeDir = Path.Combine("generated", projectId.ToString("N"));
        var absoluteDir = Path.Combine(_root, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var latestRelative = Path.Combine(relativeDir, "latest.pdf");
        var stampedRelative = Path.Combine(relativeDir, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{safeName}");

        await File.WriteAllBytesAsync(Path.Combine(_root, latestRelative), pdfBytes, cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(_root, stampedRelative), pdfBytes, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(absoluteDir, "latest-name.txt"), safeName, cancellationToken);

        return latestRelative.Replace('\\', '/');
    }

    public string? GetLatestGeneratedPdfPath(Guid projectId)
    {
        var relative = Path.Combine("generated", projectId.ToString("N"), "latest.pdf");
        return GetAbsolutePath(relative.Replace('\\', '/'));
    }

    public string? GetLatestGeneratedFileName(Guid projectId)
    {
        var nameFile = Path.Combine(_root, "generated", projectId.ToString("N"), "latest-name.txt");
        return File.Exists(nameFile) ? File.ReadAllText(nameFile).Trim() : null;
    }

    public void DeleteIfExists(string? relativePath)
    {
        var absolute = GetAbsolutePath(relativePath);
        if (absolute is not null)
            File.Delete(absolute);
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "file.bin" : name;
    }
}
