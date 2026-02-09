using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserAdminRepository
{
    Task<List<User>> GetUsersAsync(string? sortBy, string? direction, CancellationToken ct);
    Task<int> SetStatusAsync(IEnumerable<Guid> ids, Domain.Enums.UserStatus status, CancellationToken ct);
    Task<int> DeleteUsersAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<int> DeleteAllUnverifiedAsync(CancellationToken ct);
}
