using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniBusiness.Domain.Entities;
using MiniBusiness.Domain.Events;

namespace MiniBusiness.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext> _logger;
    
    public DbSet<Customer> Customers => Set<Customer>();

    public AppDbContext(DbContextOptions<AppDbContext> options,
        ILogger<AppDbContext> logger) : base(options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        // Determine CreateAt and UpdatedAt
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
        
        // Ensure transaction is success
        var result = await base.SaveChangesAsync(cancellationToken);

        // Logging domain events
        var entitiesWithEvents = base.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();
        
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case CustomerCreatedEvent createdEvent:
                    _logger.LogInformation("[Domain Event] The new customer created - ID: {CreatedEventCustomerId}, Email: {CreatedEventCustomerEmail}", createdEvent.CustomerId, createdEvent.CustomerEmail);
                    break;
                case CustomerUpdatedEvent updatedEvent:
                    _logger.LogInformation("[Domain Event] The customer information updated - ID: {UpdatedEventCustomerId}", updatedEvent.CustomerId);
                    break;
                case CustomerDeactivatedEvent deactivatedEvent:
                    _logger.LogInformation("[Domain Event] The customer account deactivated - ID: {DeactivatedCustomerId}", deactivatedEvent.CustomerId);
                    break;
                case CustomerReactivatedEvent reactivateEvent:
                    _logger.LogInformation("[Domain Event] The customer account reactivated - ID: {ReactivatedCustomerId}", reactivateEvent.CustomerId);
                    break;
            }
        }

        return result;
    }
}