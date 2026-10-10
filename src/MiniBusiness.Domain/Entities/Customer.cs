using MiniBusiness.Domain.Enums;
using MiniBusiness.Domain.Exceptions;
using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.Domain.Entities;

public class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public CustomerStatus Status { get; private set; }
    
    private Customer(){}

    public Customer(string name, Email email)
    {
        Name = name;
        Email = email;
        Status = CustomerStatus.Active;
    }

    public void Update(string name, Email email)
    {
        Name = name;
        Email = email;
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
        {
            throw new DomainException("Customer is already inactive.");
        }
        
        Status = CustomerStatus.Inactive;

    }
    
    public void Reactivate()
    {
        if (Status == CustomerStatus.Active)
        {
            throw new DomainException("Customer is already active.");
        }
        Status = CustomerStatus.Active;
    }
}