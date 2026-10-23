using Microsoft.EntityFrameworkCore;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Infrastructure.Persistence;

namespace MiniBusiness.Infrastructure.Queries;

public sealed class CustomerQueries : ICustomerQueries
{
    private readonly AppDbContext _dbContext;

    public CustomerQueries(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<CustomerDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CustomerDto(
                x.Id,
                x.Name,
                x.Email,
                x.Status.ToString(),
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var query = _dbContext.Customers
            .AsNoTracking();

        var totalCount = await query.CountAsync(
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CustomerDto(
                x.Id,
                x.Name,
                x.Email,
                x.Status.ToString(),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerDto>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages);
    }
}