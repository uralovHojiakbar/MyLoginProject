using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailQueue _emails;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IEmailQueue emails,
        IConfiguration config)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _emails = emails;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var confirmToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email.Trim().ToLowerInvariant(),
            Name = req.Name.Trim(),
            PasswordHash = _hasher.Hash(req.Password),
            Status = UserStatus.Unverified,
            CreatedAtUtc = DateTime.UtcNow,
            EmailConfirmationToken = confirmToken
        };

        await _users.AddAsync(user, ct);

        try
        {
            await _users.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // Unique index (Email) buzilsa shu yerga tushadi
            throw new InvalidOperationException("Email allaqachon mavjud.", ex);
        }

        // ✅ Render url’ni env’dan oling:
        // Render’da backend url’ingizni qo‘ying:
        // App__PublicBaseUrl = https://myloginproject-5.onrender.com
        var baseUrl = _config["App:PublicBaseUrl"]?.TrimEnd('/')
                      ?? "http://localhost:8080";

        var confirmLink = $"{baseUrl}/api/auth/confirm?userId={user.Id}&token={confirmToken}";

        _emails.Enqueue(
            user.Email,
            "Confirm your email",
            $"Salom {user.Name}!\nEmail tasdiqlash uchun link:\n{confirmLink}"
        );

        var jwtToken = _jwt.Generate(user);
        return new AuthResponse(jwtToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Email yoki parol xato.");

        if (user.Status == UserStatus.Blocked)
            throw new UnauthorizedAccessException("User block qilingan.");

        if (!_hasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email yoki parol xato.");

        user.LastLoginUtc = DateTime.UtcNow;
        await _users.SaveChangesAsync(ct);

        var token = _jwt.Generate(user);
        return new AuthResponse(token);
    }

    public async Task ConfirmEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            throw new InvalidOperationException("User topilmadi.");

        if (user.EmailConfirmedAtUtc is not null)
            return;

        if (string.IsNullOrWhiteSpace(user.EmailConfirmationToken) ||
            user.EmailConfirmationToken != token)
            throw new InvalidOperationException("Token noto‘g‘ri.");

        user.EmailConfirmedAtUtc = DateTime.UtcNow;
        user.EmailConfirmationToken = null;
        user.Status = UserStatus.Active;

        await _users.SaveChangesAsync(ct);
    }
}
