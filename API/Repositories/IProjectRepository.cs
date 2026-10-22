using API.Models;

namespace API.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default);
    Task<Project?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default);
    Task<int> DeleteForClientAsync(Guid clientId, string clientName, string ownerUserId, CancellationToken cancellationToken = default);
}
