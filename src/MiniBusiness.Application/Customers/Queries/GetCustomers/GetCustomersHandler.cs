using MediatR;
using Microsoft.Extensions.Options;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Common.Options;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerQueries _queries;
    private readonly IOptions<PaginationOptions> _paginationOptions;

    public GetCustomersHandler(ICustomerQueries queries, IOptions<PaginationOptions> paginationOptions)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _paginationOptions = paginationOptions ?? throw new ArgumentNullException(nameof(paginationOptions));
    }

    public Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? _paginationOptions.Value.MinPage : request.Page;
        var pageSize = request.PageSize <= 0 ? _paginationOptions.Value.DefaultPageSize : request.PageSize;

        return _queries.GetAllAsync(page, pageSize, cancellationToken);
    }
}