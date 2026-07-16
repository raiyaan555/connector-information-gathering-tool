using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext _db;

    public AttachmentRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Attachment>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _db.Attachments
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Attachments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync(cancellationToken);
        return attachment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await _db.Attachments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (attachment is null) return false;
        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
