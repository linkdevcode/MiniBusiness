using MediatR;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email
) : IRequest<CustomerDto>;