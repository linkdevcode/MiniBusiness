using MediatR;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersHandler
    : IRequestHandler<
        GetCustomersQuery,
        PagedResult<CustomerDto>>
{
    private readonly ICustomerQueries _queries;

    public GetCustomersHandler(ICustomerQueries queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    public Task<PagedResult<CustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        return _queries.GetAllAsync(
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}