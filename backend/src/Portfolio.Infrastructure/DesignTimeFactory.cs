using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Portfolio.Infrastructure;

/// <summary>Lets `dotnet ef` create the context without running the API.</summary>
public class DesignTimeFactory : IDesignTimeDbContextFactory<PortfolioDbContext>
{
    public PortfolioDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? "Server=localhost,1433;Database=Portfolio;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=True";
        return new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlServer(cs).Options);
    }
}
