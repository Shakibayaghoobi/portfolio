using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PortfolioDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("Default"), sql => sql.EnableRetryOnFailure()));
        return services;
    }
}
