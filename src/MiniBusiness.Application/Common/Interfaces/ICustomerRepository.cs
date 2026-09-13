using MiniBusiness.Domain.Entities;

namespace MiniBusiness.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task AddAsync(Customer  customer, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
}