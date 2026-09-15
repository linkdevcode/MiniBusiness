using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Common.Interfaces;

public interface ICustomerQueries
{
    Task<CustomerDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResult<CustomerDto>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}