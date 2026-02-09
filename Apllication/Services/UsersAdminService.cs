using Application.Common.Interfaces;
using Application.DTOs.Users;
using Domain.Enums;

namespace Application.Services;

public class UsersAdminService
{
    private readonly IUserAdminRepository _repo;

    public UsersAdminService(IUserAdminRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserDto>> GetUsersAsync(string? sortBy, string? direction, CancellationToken ct)
    {
        var users = await _repo.GetUsersAsync(sortBy, direction, ct);

        return users.Select(u => new UserDto(
            u.Id, u.Email, u.Name, u.Status, u.CreatedAtUtc, u.LastLoginUtc
        )).ToList();
    }

    public Task<int> BlockAsync(IEnumerable<Guid> ids, CancellationToken ct)
        => _repo.SetStatusAsync(ids, UserStatus.Blocked, ct);

    public Task<int> UnblockAsync(IEnumerable<Guid> ids, CancellationToken ct)
        => _repo.SetStatusAsync(ids, UserStatus.Active, ct);

    public Task<int> DeleteAsync(IEnumerable<Guid> ids, CancellationToken ct)
        => _repo.DeleteUsersAsync(ids, ct);

    public Task<int> DeleteAllUnverifiedAsync(CancellationToken ct)
        => _repo.DeleteAllUnverifiedAsync(ct);
}
