namespace API.Services;

using API.DTOs;
using API.Models;
using API.Repositories;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;

    public ClientService(IClientRepository clientRepository, IProjectRepository projectRepository)
    {
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
    }

    public ApiResponse<List<ClientDto>> GetAll()
    {
        var clients = _clientRepository.GetAll().Select(ToDto).ToList();
        return ApiResponse<List<ClientDto>>.Ok(clients);
    }

    public ApiResponse<ClientDto> Create(CreateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return ApiResponse<ClientDto>.Fail("Company name is required.");

        var client = new Client
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName.Trim(),
            Industry = request.Industry.Trim(),
            PrimaryContact = request.PrimaryContact.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Country = request.Country.Trim(),
            Address = request.Address.Trim(),
            Notes = request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _clientRepository.Add(client);
        return ApiResponse<ClientDto>.Ok(ToDto(client), "Client created successfully.");
    }

    public ApiResponse<MessageResponse> Delete(Guid id)
    {
        var client = _clientRepository.GetById(id);
        if (client is null)
            return ApiResponse<MessageResponse>.Fail("Client not found.");

        _projectRepository.DeleteByClientName(client.CompanyName);

        if (!_clientRepository.Delete(id))
            return ApiResponse<MessageResponse>.Fail("Client not found.");

        return ApiResponse<MessageResponse>.Ok(
            new MessageResponse { Message = "Client deleted successfully." },
            "Client deleted successfully.");
    }

    private static ClientDto ToDto(Client client) =>
        new()
        {
            Id = client.Id,
            CompanyName = client.CompanyName,
            Industry = client.Industry,
            PrimaryContact = client.PrimaryContact,
            Email = client.Email,
            Phone = client.Phone,
            Country = client.Country,
            Address = client.Address,
            Notes = client.Notes,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
}
