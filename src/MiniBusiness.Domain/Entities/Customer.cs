using MiniBusiness.Domain.Enums;
using MiniBusiness.Domain.Exceptions;

namespace MiniBusiness.Domain.Entities;

public class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public CustomerStatus Status { get; private set; }
    
    private Customer(){}

    public Customer(string name, string email)
    {
        Name = name;
        Email = email;
        Status = CustomerStatus.Active;
    }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Customer is already inactive.");

        Status = CustomerStatus.Inactive;

    }
    
    public void Active()
    {
        Status = CustomerStatus.Active;
    }
}