using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Auth;
using Portfolio.Domain;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers.Admin;

public record LoginRequest(string Email, string Password);

[ApiController, Route("api/admin/auth")]
public class AuthController(PortfolioDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email, ct);
        var hasher = new PasswordHasher<AdminUser>();
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { error = "Invalid email or password." });
        user.LastLoginUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var (token, expires) = tokens.Create(user);
        return Ok(new { token, expiresUtc = expires });
    }

    [HttpGet("me"), Authorize(Roles = "Admin")]
    public IActionResult Me() => Ok(new { email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value });
}
