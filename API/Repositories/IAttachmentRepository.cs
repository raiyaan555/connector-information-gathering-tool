using API.Models;

namespace API.Repositories;

public interface IAttachmentRepository
{
    Task<IReadOnlyList<Attachment>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
