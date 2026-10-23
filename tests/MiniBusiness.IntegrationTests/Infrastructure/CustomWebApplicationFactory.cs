using System.Data.Common;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniBusiness.Application.Common.Options;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using MiniBusiness.Infrastructure.Persistence;

namespace MiniBusiness.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres -d test_db"))
        .Build();

    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public CustomWebApplicationFactory()
    {
        
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            GetTestConnectionString());

        builder.ConfigureServices(services =>
        {
            services.Configure<PaginationOptions>(options =>
            {
                options.MinPage = 1;
                options.DefaultPageSize = 10;
                options.MinPageSize = 1;
                options.MaxPageSize = 100;
            });
            
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Lấy ConnectionString từ Container đã được Start
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(GetTestConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        // 1. Start Postgres Container
        await _postgres.StartAsync();

        // 2. Chạy Migration 1 lần duy nhất cho toàn bộ Test Suite
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        // 3. Khởi tạo Respawner
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

    private string GetTestConnectionString() => _postgres.GetConnectionString() + ";SSL Mode=Disable";
}