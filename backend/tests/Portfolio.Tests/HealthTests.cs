using Microsoft.AspNetCore.Hosting;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.Tests;

public class HealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_returns_ok()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", new string('k', 40));
        var client = factory.WithWebHostBuilder(b => b.UseEnvironment("Testing")).CreateClient();
        var res = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Admin_endpoints_require_auth()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", new string('k', 40));
        var client = factory.WithWebHostBuilder(b => b.UseEnvironment("Testing")).CreateClient();
        var res = await client.GetAsync("/api/admin/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
