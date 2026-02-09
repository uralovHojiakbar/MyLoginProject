using Domain.Enums;

namespace Application.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Name,
    UserStatus Status,
    DateTime CreatedAtUtc,
    DateTime? LastLoginUtc
);
