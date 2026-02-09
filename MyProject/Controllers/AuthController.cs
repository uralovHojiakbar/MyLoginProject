using Application.DTOs.Auth;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req, CancellationToken ct)
        => Ok(await _auth.RegisterAsync(req, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req, CancellationToken ct)
        => Ok(await _auth.LoginAsync(req, ct));
    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
    {
        await _auth.ConfirmEmailAsync(userId, token, ct);
        return Ok("Email confirmed. Account is Active now.");
    }

}
