using System.Security.Claims;
using API.DTOs;
using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Http;

namespace API.Services;

public interface IAttachmentService
{
    Task<ApiResponse<IEnumerable<AttachmentDto>>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ApiResponse<AttachmentDto>> UploadAsync(Guid projectId, UploadAttachmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AttachmentDto>> UploadFileAsync(Guid projectId, IFormFile file, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class AttachmentService : IAttachmentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AttachmentService(
        IProjectRepository projectRepository,
        IAttachmentRepository attachmentRepository,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContextAccessor)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<IEnumerable<AttachmentDto>>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<IEnumerable<AttachmentDto>>.Fail("Project not found.");

        var attachments = await _attachmentRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return ApiResponse<IEnumerable<AttachmentDto>>.Ok(attachments.Select(MapToDto));
    }

    public async Task<ApiResponse<AttachmentDto>> UploadAsync(
        Guid projectId,
        UploadAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<AttachmentDto>.Fail("Project not found.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return ApiResponse<AttachmentDto>.Fail("File name is required.");

        var uploadedBy = GetUploader();
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize > 0 ? request.FileSize : 1024,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);
        return ApiResponse<AttachmentDto>.Ok(MapToDto(attachment), "File uploaded successfully.");
    }

    public async Task<ApiResponse<AttachmentDto>> UploadFileAsync(
        Guid projectId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<AttachmentDto>.Fail("Project not found.");

        if (file is null || file.Length == 0)
            return ApiResponse<AttachmentDto>.Fail("File is required.");

        var id = Guid.NewGuid();
        await using var stream = file.OpenReadStream();
        var storagePath = await _fileStorage.SaveAttachmentAsync(
            projectId, id, file.FileName, stream, cancellationToken);

        var attachment = new Attachment
        {
            Id = id,
            ProjectId = projectId,
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSize = file.Length,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = GetUploader()
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);
        return ApiResponse<AttachmentDto>.Ok(MapToDto(attachment), "File uploaded successfully.");
    }

    public async Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _attachmentRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        _fileStorage.DeleteIfExists(existing.StoragePath);

        if (!await _attachmentRepository.DeleteAsync(id, cancellationToken))
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Attachment deleted successfully." },
            "Attachment deleted successfully.");
    }

    private string GetUploader() =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? "system@arconnet.com";

    private static AttachmentDto MapToDto(Attachment attachment) => new()
    {
        Id = attachment.Id,
        ProjectId = attachment.ProjectId,
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        FileSize = attachment.FileSize,
        UploadedAt = attachment.UploadedAt,
        UploadedBy = attachment.UploadedBy
    };
}
