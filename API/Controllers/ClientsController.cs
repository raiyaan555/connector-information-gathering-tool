namespace API.Controllers;

using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<List<ClientDto>>> GetAll()
    {
        return Ok(_clientService.GetAll());
    }

    [HttpPost]
    public ActionResult<ApiResponse<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        var result = _clientService.Create(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<ApiResponse<MessageResponse>> Delete(Guid id)
    {
        var result = _clientService.Delete(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

