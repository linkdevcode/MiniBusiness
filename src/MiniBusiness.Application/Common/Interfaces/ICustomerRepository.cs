using MiniBusiness.Domain.Entities;

namespace MiniBusiness.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken);

    Task<Customer?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailExceptAsync(
        string email,
        Guid customerId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}