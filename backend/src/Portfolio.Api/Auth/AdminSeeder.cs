using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Auth;

public static class AdminSeeder
{
    /// <summary>Creates the first admin from Admin:Email / Admin:Password config when no admin exists.</summary>
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config, ILogger logger)
    {
        var db = sp.GetRequiredService<PortfolioDbContext>();
        if (await db.AdminUsers.AnyAsync()) return;
        var email = config["Admin:Email"]; var password = config["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin user exists and Admin:Email/Admin:Password are not configured.");
            return;
        }
        var user = new AdminUser { Email = email.Trim().ToLowerInvariant() };
        user.PasswordHash = new PasswordHasher<AdminUser>().HashPassword(user, password);
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded admin user {Email}", user.Email);
    }
}
