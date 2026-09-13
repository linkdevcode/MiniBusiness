using MiniBusiness.Domain.Enums;

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

    public void Deactive()
    {
        Status = CustomerStatus.Inactive;
    }
    
    public void Active()
    {
        Status = CustomerStatus.Active;
    }
}