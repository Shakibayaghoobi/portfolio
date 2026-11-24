using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Api.Auth;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Section));
builder.Services.AddSingleton<TokenService>();
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (jwt.Key.Length < 32) throw new InvalidOperationException("Jwt:Key must be at least 32 characters (set env var Jwt__Key or user-secrets).");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ValidateIssuerSigningKey = true, ClockSkew = TimeSpan.FromMinutes(1),
    });
builder.Services.AddAuthorization();

var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.MigrateAsync();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
    await ContentSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<PortfolioDbContext>(), app.Logger);
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
