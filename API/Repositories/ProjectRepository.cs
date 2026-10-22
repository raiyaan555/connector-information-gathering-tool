using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _db;

    public ProjectRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Project>> GetAllAsync(string ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerUserId == ownerUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _db.Projects.FirstOrDefaultAsync(
            p => p.Id == id && p.OwnerUserId == ownerUserId,
            cancellationToken);
    }

    public async Task<Project?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _db.Projects.FirstOrDefaultAsync(p => p.FormToken == token, cancellationToken);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Update(project);
        await _db.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<bool> DeleteAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(
            p => p.Id == id && p.OwnerUserId == ownerUserId,
            cancellationToken);
        if (project is null) return false;
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> DeleteForClientAsync(
        Guid clientId,
        string clientName,
        string ownerUserId,
        CancellationToken cancellationToken = default)
    {
        var name = clientName.ToLower();
        var projects = await _db.Projects
            .Where(p => p.OwnerUserId == ownerUserId
                && (p.ClientId == clientId || (p.ClientId == null && p.ClientName.ToLower() == name)))
            .ToListAsync(cancellationToken);

        if (projects.Count == 0) return 0;
        _db.Projects.RemoveRange(projects);
        await _db.SaveChangesAsync(cancellationToken);
        return projects.Count;
    }
}
