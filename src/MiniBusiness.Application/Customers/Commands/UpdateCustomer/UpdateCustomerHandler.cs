using MediatR;
using MiniBusiness.Application.Common.Interfaces;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateCustomerHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdForUpdateAsync(
                request.Id, cancellationToken);

        if (customer is null)
            throw new KeyNotFoundException("Customer was not found.");

        var email = Email.Create(request.Email);
        var emailExists = await _customerRepository.ExistsByEmailExceptAsync(
            email.Value, request.Id, cancellationToken);

        if (emailExists)
            throw new InvalidOperationException("A customer with this email already exists.");

        customer.Update(request.Name, email);

        await _customerRepository.SaveChangesAsync(cancellationToken);

        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email.Value,
            customer.Status.ToString(),
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}