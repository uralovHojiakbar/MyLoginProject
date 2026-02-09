using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class UserAdminRepository : IUserAdminRepository
{
    private readonly AppDbContext _db;

    public UserAdminRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetUsersAsync(string? sortBy, string? direction, CancellationToken ct)
    {
        var desc = (direction ?? "desc").Equals("desc", StringComparison.OrdinalIgnoreCase);
        var s = (sortBy ?? "lastLogin").Trim().ToLowerInvariant();

        IQueryable<User> q = _db.Users.AsNoTracking();

        // default: last login desc (null'lar oxiriga ketadi)
        q = s switch
        {
            "email" => desc ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email),
            "name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            "created" => desc ? q.OrderByDescending(x => x.CreatedAtUtc) : q.OrderBy(x => x.CreatedAtUtc),
            _ => desc
                ? q.OrderByDescending(x => x.LastLoginUtc.HasValue)
                     .ThenByDescending(x => x.LastLoginUtc)
                : q.OrderByDescending(x => x.LastLoginUtc.HasValue)
                     .ThenBy(x => x.LastLoginUtc),
        };

        return await q.ToListAsync(ct);
    }

    public async Task<int> SetStatusAsync(IEnumerable<Guid> ids, UserStatus status, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return 0;

        var users = await _db.Users.Where(x => idList.Contains(x.Id)).ToListAsync(ct);

        foreach (var u in users)
            u.Status = status;

        return await _db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteUsersAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return 0;

        var users = await _db.Users.Where(x => idList.Contains(x.Id)).ToListAsync(ct);

        _db.Users.RemoveRange(users);
        return await _db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAllUnverifiedAsync(CancellationToken ct)
    {
        var unverified = await _db.Users.Where(x => x.Status == UserStatus.Unverified).ToListAsync(ct);

        _db.Users.RemoveRange(unverified);
        return await _db.SaveChangesAsync(ct);
    }
}
