using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    // 👇 MUHIM JOY SHU
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        // AsNoTracking YO‘Q !!!
        return _db.Users
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Users
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _db.Users.AddAsync(user, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
