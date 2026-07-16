using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _db;

    public ClientRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Clients
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        _db.Clients.Add(client);
        await _db.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null) return false;
        _db.Clients.Remove(client);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
