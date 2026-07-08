using API.DTOs;
using API.Models;

namespace API.Services;

public interface IAttachmentService
{
    ApiResponse<IEnumerable<AttachmentDto>> GetByProjectId(Guid projectId);
    ApiResponse<AttachmentDto> Upload(Guid projectId, UploadAttachmentRequest request);
    ApiResponse<MessageResponse> Delete(Guid id);
}
