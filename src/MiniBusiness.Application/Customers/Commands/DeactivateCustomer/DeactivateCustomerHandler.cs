using MediatR;
using MiniBusiness.Application.Common.Interfaces;

namespace MiniBusiness.Application.Customers.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerHandler : IRequestHandler<DeactivateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public DeactivateCustomerHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");

        customer.Deactivate();

        await _customerRepository.SaveChangesAsync(cancellationToken);
    }
}