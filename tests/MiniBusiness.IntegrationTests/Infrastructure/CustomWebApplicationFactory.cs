using System.Data.Common;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiniBusiness.Application.Common.Options;
using MiniBusiness.Infrastructure.Persistence;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace MiniBusiness.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres -d test_db"))
        .Build();

    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.Configure<PaginationOptions>(options =>
            {
                options.MinPage = 1;
                options.DefaultPageSize = 10;
                options.MinPageSize = 1;
                options.MaxPageSize = 100;
            });

            // WebApplicationFactory + minimal hosting: config override is unreliable —
            // replace DbContext and health check so both use the Testcontainers CS.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(GetTestConnectionString());
            });

            services.Configure<HealthCheckServiceOptions>(options =>
            {
                foreach (var registration in options.Registrations
                             .Where(r => r.Name == "postgres")
                             .ToList())
                {
                    options.Registrations.Remove(registration);
                }
            });

            services.AddHealthChecks()
                .AddNpgSql(
                    GetTestConnectionString(),
                    name: "postgres",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["ready"]);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        _dbConnection = new NpgsqlConnection(GetTestConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();

        await _postgres.StopAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    private string GetTestConnectionString() =>
        _postgres.GetConnectionString() + ";SSL Mode=Disable";
}
