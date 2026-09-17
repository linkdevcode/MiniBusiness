using FluentAssertions;
using MiniBusiness.Domain.Entities;
using MiniBusiness.Domain.Enums;
using MiniBusiness.Domain.Exceptions;

namespace MiniBusiness.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Constructor_WhenCreated_StatusIsActive()
    {
        // Arrange
        const string name = "Nguyen Van A";
        const string email = "a@example.com";

        // Act
        var customer = new Customer(name, email);

        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be(name);
        customer.Email.Should().Be(email);
        customer.Status.Should().Be(CustomerStatus.Active);
    }
    
    [Fact]
    public void Deactivate_WhenActive_SetsStatusToInactive()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", "a@example.com");

        // Act
        customer.Deactivate();

        // Assert
        customer.Status.Should().Be(CustomerStatus.Inactive);
    }
    
    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsDomainException()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", "a@example.com");
        customer.Deactivate();
        
        // Act
        var action = () => customer.Deactivate();

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Customer is already inactive.");
    }

    [Fact]
    public void Update_WhenCalled_ChangesNameAndEmail()
    {
        // Arrange
        var customer = new Customer("Old Name", "old@example.com");
        const string newName = "New Name";
        const string newEmail = "new@example.com";

        // Act
        customer.Update(newName, newEmail);

        // Assert
        customer.Name.Should().Be(newName);
        customer.Email.Should().Be(newEmail);
        customer.Status.Should().Be(CustomerStatus.Active);
    }
}
