using MediatR;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Application.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdHandler
    : IRequestHandler<
        GetCustomerByIdQuery,
        CustomerDto?>
{
    private readonly ICustomerQueries _queries;

    public GetCustomerByIdHandler(ICustomerQueries queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    public async Task<CustomerDto?> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _queries.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}