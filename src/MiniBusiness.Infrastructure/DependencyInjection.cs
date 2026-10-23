using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Infrastructure.Persistence;
using MiniBusiness.Infrastructure.Queries;
using MiniBusiness.Infrastructure.Repositories;

namespace MiniBusiness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString!,
                name: "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerQueries, CustomerQueries>();
        
        return services;
    }
}