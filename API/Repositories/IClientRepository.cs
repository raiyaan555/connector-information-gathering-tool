using API.Models;

namespace API.Repositories;

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> GetAllAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default);
    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default);
}
