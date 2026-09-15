using MediatR;

namespace MiniBusiness.Application.Customers.Commands.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(
    Guid Id
) : IRequest;