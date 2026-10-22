using System.Security.Claims;
using API.DTOs;
using API.Helpers;
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
    private readonly ICurrentUser _currentUser;

    public AttachmentService(
        IProjectRepository projectRepository,
        IAttachmentRepository attachmentRepository,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUser currentUser)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<IEnumerable<AttachmentDto>>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await OwnedProjectAsync(projectId, cancellationToken);
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
        var project = await OwnedProjectAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<AttachmentDto>.Fail("Project not found.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return ApiResponse<AttachmentDto>.Fail("File name is required.");

        if (!AttachmentFileRules.IsAllowedExtension(request.FileName))
            return ApiResponse<AttachmentDto>.Fail(AttachmentFileRules.RejectionMessage);

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
        var project = await OwnedProjectAsync(projectId, cancellationToken);
        if (project is null)
            return ApiResponse<AttachmentDto>.Fail("Project not found.");

        if (file is null || file.Length == 0)
            return ApiResponse<AttachmentDto>.Fail("File is required.");

        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
            return ApiResponse<AttachmentDto>.Fail("File name is required.");

        await using var source = file.OpenReadStream();
        var ownsBuffer = false;
        Stream content = source;
        if (!source.CanSeek)
        {
            var buffer = new MemoryStream();
            await source.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;
            content = buffer;
            ownsBuffer = true;
        }

        try
        {
            if (!AttachmentFileRules.TryValidate(fileName, file.ContentType, content, out var contentType, out var error))
                return ApiResponse<AttachmentDto>.Fail(error);

            content.Position = 0;
            var id = Guid.NewGuid();
            var storagePath = await _fileStorage.SaveAttachmentAsync(
                projectId, id, fileName, content, cancellationToken);

            var attachment = new Attachment
            {
                Id = id,
                ProjectId = projectId,
                FileName = fileName,
                ContentType = contentType,
                FileSize = file.Length,
                StoragePath = storagePath,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = GetUploader()
            };

            await _attachmentRepository.AddAsync(attachment, cancellationToken);
            return ApiResponse<AttachmentDto>.Ok(MapToDto(attachment), "File uploaded successfully.");
        }
        finally
        {
            if (ownsBuffer)
                await content.DisposeAsync();
        }
    }

    public async Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _attachmentRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        var project = await OwnedProjectAsync(existing.ProjectId, cancellationToken);
        if (project is null)
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        _fileStorage.DeleteIfExists(existing.StoragePath);

        if (!await _attachmentRepository.DeleteAsync(id, cancellationToken))
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Attachment deleted successfully." },
            "Attachment deleted successfully.");
    }

    private async Task<Project?> OwnedProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var ownerId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(ownerId))
            return null;

        return await _projectRepository.GetByIdAsync(projectId, ownerId, cancellationToken);
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
