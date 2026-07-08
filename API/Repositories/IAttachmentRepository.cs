namespace API.Repositories;

using API.Models;

public interface IAttachmentRepository
{
    IReadOnlyList<Attachment> GetByProjectId(Guid projectId);
    Attachment? GetById(Guid id);
    Attachment Add(Attachment attachment);
    bool Delete(Guid id);
}
