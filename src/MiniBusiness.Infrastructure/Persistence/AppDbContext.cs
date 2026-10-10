using Microsoft.EntityFrameworkCore;
using MiniBusiness.Domain.Entities;

namespace MiniBusiness.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var utcNow = DateTimeOffset.UtcNow;
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e =>
                e.State == EntityState.Added || 
                e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(Entity.CreatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(Entity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = utcNow;
                    break;
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}