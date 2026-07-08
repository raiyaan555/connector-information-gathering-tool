using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/customer-form")]
public class CustomerFormController : ControllerBase
{
    private readonly ICustomerFormService _customerFormService;

    public CustomerFormController(ICustomerFormService customerFormService)
    {
        _customerFormService = customerFormService;
    }

    [HttpGet("{token}")]
    public ActionResult<ApiResponse<CustomerFormDto>> GetForm(string token)
    {
        var result = _customerFormService.GetFormByToken(token);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{token}")]
    public ActionResult<ApiResponse<CustomerFormResponseDto>> SubmitForm(
        string token,
        [FromBody] SubmitCustomerFormRequest request)
    {
        var result = _customerFormService.SubmitForm(token, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("project/{projectId:guid}/responses")]
    public ActionResult<ApiResponse<IEnumerable<CustomerFormResponseDto>>> GetResponses(Guid projectId)
    {
        var result = _customerFormService.GetResponsesByProjectId(projectId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
