using MediatR;
using MiniBusiness.Application.Customers.DTOs;

namespace MiniBusiness.Application.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string Email) : IRequest<CustomerDto>;