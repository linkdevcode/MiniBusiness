using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Domain.Entities;

namespace MiniBusiness.UnitTests.Fakes;

public sealed class FakeCustomerRepository : ICustomerRepository
{
    public bool ExistsByEmailResult { get; set; }
    public bool ExistsByEmailExceptResult { get; set; }
    public int AddCallCount { get; private set; }
    public int SaveChangesCallCount { get; private set; }
    public Customer? CustomerById { get; set; }
    
    
    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        AddCallCount++;
        return Task.CompletedTask;
    }

    public Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(CustomerById);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(ExistsByEmailResult);
    }

    public Task<bool> ExistsByEmailExceptAsync(string email, Guid customerId, CancellationToken cancellationToken)
    {
        return Task.FromResult(ExistsByEmailExceptResult);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}