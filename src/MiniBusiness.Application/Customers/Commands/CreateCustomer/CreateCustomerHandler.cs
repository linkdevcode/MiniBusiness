using MediatR;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Domain.Entities;

namespace MiniBusiness.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerHandler
    : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository  _customerRepository;

    public CreateCustomerHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ??  throw new ArgumentNullException(nameof(customerRepository));
    }
    
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _customerRepository.ExistsByEmailAsync(request.Email, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("A customer with this email already exists.");
        }

        var customer = new Customer(request.Name, request.Email);

        await _customerRepository.AddAsync(customer, cancellationToken);

        return new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Status.ToString());
    }
}
