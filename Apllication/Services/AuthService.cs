using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailQueue _emails;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        IEmailQueue emails)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
        _emails = emails;
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
        catch (Exception ex)
        {
            // Unique index buzilsa (duplicate email) shu yerga tushadi
            throw new InvalidOperationException("Email allaqachon mavjud.", ex);
        }

        // Async email queue (MOCK sender bo'lsa ham bo'ladi)
        // Porting sendeda boshqacha bo'lsa (https port), keyin configdan olamiz.
        var confirmLink = $"https://localhost:5001/api/auth/confirm?userId={user.Id}&token={confirmToken}";

        _emails.Enqueue(
            user.Email,
            "Confirm your email",
            $"Salom {user.Name}!\nEmail tasdiqlash uchun linkni bosing:\n{confirmLink}"
        );

        // Task talabi bo'yicha email tasdiqlanmaguncha ko'p endpointlarga ruxsat bermaymiz
        // buni middleware qilyapti. Lekin JWT berib yuboramiz (foydalanuvchi confirm qiladi).
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

        // allaqachon confirmed bo'lsa qaytamiz
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
