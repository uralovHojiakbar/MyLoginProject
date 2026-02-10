using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public UserStatus Status { get; set; } = UserStatus.Unverified;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }

    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmedAtUtc { get; set; }
}
