using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Infrastructure.Persistence;
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

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}