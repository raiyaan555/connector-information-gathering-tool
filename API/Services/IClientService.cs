namespace API.Services;

using API.DTOs;
using API.Models;

public interface IClientService
{
    ApiResponse<List<ClientDto>> GetAll();
    ApiResponse<ClientDto> Create(CreateClientRequest request);
    ApiResponse<MessageResponse> Delete(Guid id);
}

