using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}")]
public class PdfController : ControllerBase
{
    private readonly IConnectorPdfService _pdfService;

    public PdfController(IConnectorPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpPost("generate-pdf")]
    public async Task<IActionResult> GeneratePdf(
        Guid projectId,
        [FromBody] GeneratePdfRequest request,
        CancellationToken cancellationToken)
    {
        var (success, error, pdfBytes, fileName) = await _pdfService.GenerateAsync(
            projectId,
            request.FormData,
            cancellationToken);

        if (!success || pdfBytes is null)
            return BadRequest(ApiResponse<object>.Fail(error ?? "PDF generation failed."));

        return File(pdfBytes, "application/pdf", fileName ?? "ConnectorDocument.pdf");
    }

    [HttpPost("share-email")]
    public async Task<IActionResult> ShareEmail(Guid projectId, CancellationToken cancellationToken)
    {
        var (success, error, emlBytes, fileName) = await _pdfService.BuildOutlookDraftAsync(
            projectId,
            cancellationToken);

        if (!success || emlBytes is null)
            return BadRequest(ApiResponse<object>.Fail(error ?? "Unable to create email draft."));

        return File(emlBytes, "message/rfc822", fileName ?? "share.eml");
    }
}
