namespace API.Repositories;

using API.Models;

public class InMemoryAttachmentRepository : IAttachmentRepository
{
    private static readonly List<Attachment> Attachments =
    [
        new Attachment
        {
            Id = Guid.Parse("b1000000-0000-0000-0000-000000000001"),
            ProjectId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
            FileName = "integration-spec.pdf",
            ContentType = "application/pdf",
            FileSize = 245760,
            UploadedAt = DateTime.UtcNow.AddDays(-10),
            UploadedBy = "admin@theconnector.com"
        },
        new Attachment
        {
            Id = Guid.Parse("b1000000-0000-0000-0000-000000000002"),
            ProjectId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
            FileName = "api-credentials.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileSize = 18432,
            UploadedAt = DateTime.UtcNow.AddDays(-8),
            UploadedBy = "admin@theconnector.com"
        },
        new Attachment
        {
            Id = Guid.Parse("b1000000-0000-0000-0000-000000000003"),
            ProjectId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
            FileName = "data-mapping.csv",
            ContentType = "text/csv",
            FileSize = 8192,
            UploadedAt = DateTime.UtcNow.AddDays(-20),
            UploadedBy = "admin@theconnector.com"
        }
    ];
    private static readonly object Lock = new();

    public IReadOnlyList<Attachment> GetByProjectId(Guid projectId)
    {
        lock (Lock)
        {
            return Attachments
                .Where(a => a.ProjectId == projectId)
                .OrderByDescending(a => a.UploadedAt)
                .ToList();
        }
    }

    public Attachment? GetById(Guid id)
    {
        lock (Lock) return Attachments.FirstOrDefault(a => a.Id == id);
    }

    public Attachment Add(Attachment attachment)
    {
        lock (Lock)
        {
            Attachments.Add(attachment);
            return attachment;
        }
    }

    public bool Delete(Guid id)
    {
        lock (Lock)
        {
            var attachment = Attachments.FirstOrDefault(a => a.Id == id);
            if (attachment is null) return false;
            Attachments.Remove(attachment);
            return true;
        }
    }
}
