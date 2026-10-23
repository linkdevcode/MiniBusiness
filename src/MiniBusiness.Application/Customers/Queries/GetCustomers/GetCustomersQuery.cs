using MediatR;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(
    int? Page,
    int? PageSize
) : IRequest<PagedResult<CustomerDto>>;
