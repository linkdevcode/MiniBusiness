using Microsoft.EntityFrameworkCore;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Domain.Entities;
using MiniBusiness.Infrastructure.Persistence;

namespace MiniBusiness.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext  _dbContext;
    
    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(x => x.Email == email, cancellationToken);
    }
}