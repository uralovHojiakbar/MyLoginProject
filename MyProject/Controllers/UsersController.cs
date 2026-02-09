using Application.DTOs.Users;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UsersAdminService _service;

    public UsersController(UsersAdminService service)
    {
        _service = service;
    }

    // GET /api/users?sortBy=lastLogin&direction=desc
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> Get([FromQuery] string? sortBy, [FromQuery] string? direction, CancellationToken ct)
        => Ok(await _service.GetUsersAsync(sortBy, direction, ct));

    [HttpPost("block")]
    public async Task<ActionResult> Block(IdsRequest req, CancellationToken ct)
        => Ok(new { updated = await _service.BlockAsync(req.Ids, ct) });

    [HttpPost("unblock")]
    public async Task<ActionResult> Unblock(IdsRequest req, CancellationToken ct)
        => Ok(new { updated = await _service.UnblockAsync(req.Ids, ct) });

    // ko‘p client DELETE body yuborolmagani uchun POST qildik
    [HttpPost("delete")]
    public async Task<ActionResult> Delete(IdsRequest req, CancellationToken ct)
        => Ok(new { deleted = await _service.DeleteAsync(req.Ids, ct) });

    [HttpDelete("unverified")]
    public async Task<ActionResult> DeleteAllUnverified(CancellationToken ct)
        => Ok(new { deleted = await _service.DeleteAllUnverifiedAsync(ct) });
}
