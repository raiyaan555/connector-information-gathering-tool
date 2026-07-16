using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttachmentDto>>>> GetByProject(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _attachmentService.GetByProjectIdAsync(projectId, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Legacy metadata-only upload (kept for compatibility).</summary>
    [HttpPost("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Upload(
        Guid projectId,
        [FromBody] UploadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attachmentService.UploadAsync(projectId, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Real file upload used for PDF consolidation.</summary>
    [HttpPost("project/{projectId:guid}/file")]
    [RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> UploadFile(
        Guid projectId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await _attachmentService.UploadFileAsync(projectId, file, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MessageResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
