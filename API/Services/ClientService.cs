using API.DTOs;
using API.Helpers;
using API.Models;
using API.Repositories;

namespace API.Services;

public interface IClientService
{
    Task<ApiResponse<List<ClientDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ClientDto>> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;

    public ClientService(IClientRepository clientRepository, IProjectRepository projectRepository)
    {
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
    }

    public async Task<ApiResponse<List<ClientDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(cancellationToken);
        return ApiResponse<List<ClientDto>>.Ok(clients.Select(ToDto).ToList());
    }

    public async Task<ApiResponse<ClientDto>> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
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

        await _clientRepository.AddAsync(client, cancellationToken);
        return ApiResponse<ClientDto>.Ok(ToDto(client), "Client created successfully.");
    }

    public async Task<ApiResponse<MessageResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client is null)
            return ApiResponse<MessageResponse>.Fail("Client not found.");

        await _projectRepository.DeleteByClientNameAsync(client.CompanyName, cancellationToken);

        if (!await _clientRepository.DeleteAsync(id, cancellationToken))
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
