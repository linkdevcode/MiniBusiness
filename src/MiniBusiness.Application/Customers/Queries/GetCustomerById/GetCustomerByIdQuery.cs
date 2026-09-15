using MediatR;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;