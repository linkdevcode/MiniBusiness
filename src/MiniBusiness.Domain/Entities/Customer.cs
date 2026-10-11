using MiniBusiness.Domain.Enums;
using MiniBusiness.Domain.Events;
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
        
        AddDomainEvent(new CustomerCreatedEvent(Id, Email.Value));
    }

    public void Update(string name, Email email)
    {
        Name = name;
        Email = email;
        
        AddDomainEvent(new CustomerUpdatedEvent(Id));
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
        {
            throw new DomainException("Customer is already inactive.");
        }
        Status = CustomerStatus.Inactive;
        
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }
    
    public void Reactivate()
    {
        if (Status == CustomerStatus.Active)
        {
            throw new DomainException("Customer is already active.");
        }
        Status = CustomerStatus.Active;
        
        AddDomainEvent(new CustomerReactivatedEvent(Id));
    }
}