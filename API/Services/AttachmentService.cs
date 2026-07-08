using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAttachmentRepository _attachmentRepository;

    public AttachmentService(
        IProjectRepository projectRepository,
        IAttachmentRepository attachmentRepository)
    {
        _projectRepository = projectRepository;
        _attachmentRepository = attachmentRepository;
    }

    public ApiResponse<IEnumerable<AttachmentDto>> GetByProjectId(Guid projectId)
    {
        var project = _projectRepository.GetById(projectId);
        if (project is null)
            return ApiResponse<IEnumerable<AttachmentDto>>.Fail("Project not found.");

        var attachments = _attachmentRepository.GetByProjectId(projectId).Select(MapToDto);
        return ApiResponse<IEnumerable<AttachmentDto>>.Ok(attachments);
    }

    public ApiResponse<AttachmentDto> Upload(Guid projectId, UploadAttachmentRequest request)
    {
        var project = _projectRepository.GetById(projectId);
        if (project is null)
            return ApiResponse<AttachmentDto>.Fail("Project not found.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return ApiResponse<AttachmentDto>.Fail("File name is required.");

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize > 0 ? request.FileSize : 1024,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = "admin@theconnector.com"
        };

        _attachmentRepository.Add(attachment);
        return ApiResponse<AttachmentDto>.Ok(MapToDto(attachment), "File uploaded successfully.");
    }

    public ApiResponse<MessageResponse> Delete(Guid id)
    {
        if (!_attachmentRepository.Delete(id))
            return ApiResponse<MessageResponse>.Fail("Attachment not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Attachment deleted successfully." },
            "Attachment deleted successfully.");
    }

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
