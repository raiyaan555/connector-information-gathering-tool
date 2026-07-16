using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class CustomerFormRepository : ICustomerFormRepository
{
    private readonly ApplicationDbContext _db;

    public CustomerFormRepository(ApplicationDbContext db) => _db = db;

    public async Task<CustomerForm?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _db.CustomerForms
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerForm>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _db.CustomerForms
            .AsNoTracking()
            .Where(f => f.ProjectId == projectId)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerForm> AddAsync(CustomerForm form, CancellationToken cancellationToken = default)
    {
        _db.CustomerForms.Add(form);
        await _db.SaveChangesAsync(cancellationToken);
        return form;
    }

    public async Task<bool> IsSubmittedAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _db.CustomerForms.AnyAsync(f => f.Token == token, cancellationToken);
    }
}
