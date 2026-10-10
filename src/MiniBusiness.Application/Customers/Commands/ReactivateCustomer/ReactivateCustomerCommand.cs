using MediatR;

namespace MiniBusiness.Application.Customers.Commands.ReactivateCustomer;

public sealed record ReactivateCustomerCommand(Guid Id) : IRequest;