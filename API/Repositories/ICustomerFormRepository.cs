using API.Models;

namespace API.Repositories;

public interface ICustomerFormRepository
{
    Task<CustomerForm?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerForm>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<CustomerForm> AddAsync(CustomerForm form, CancellationToken cancellationToken = default);
    Task<bool> IsSubmittedAsync(string token, CancellationToken cancellationToken = default);
}
