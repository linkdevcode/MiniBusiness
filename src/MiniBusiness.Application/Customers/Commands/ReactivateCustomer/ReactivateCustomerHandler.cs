using MediatR;
using MiniBusiness.Application.Common.Interfaces;

namespace MiniBusiness.Application.Customers.Commands.ReactivateCustomer;

public sealed class ReactivateCustomerHandler : IRequestHandler<ReactivateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public ReactivateCustomerHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }
    
    public async Task Handle(ReactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");
        
        customer.Reactivate();

        await _customerRepository.SaveChangesAsync(cancellationToken);
    }
}