using FluentAssertions;
using MiniBusiness.Domain.Entities;
using MiniBusiness.Domain.Enums;
using MiniBusiness.Domain.Exceptions;
using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void OK_Constructor_WhenCreated_StatusIsActive()
    {
        // Arrange
        var name = "Nguyen Van A";
        var email = Email.Create("a@example.com");

        // Act
        var customer = new Customer(name, email);

        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be(name);
        customer.Email.Should().Be(email);
        customer.Status.Should().Be(CustomerStatus.Active);
    }
    
    [Fact]
    public void OK_Deactivate_WhenActive_SetsStatusToInactive()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", Email.Create("a@example.com"));

        // Act
        customer.Deactivate();

        // Assert
        customer.Status.Should().Be(CustomerStatus.Inactive);
    }

    [Fact]
    public void OK_Reactivate_WhenDeactivate_SetsStatusToActive()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", Email.Create("a@example.com"));
        customer.Deactivate();
        
        // Act
        customer.Reactivate();

        // Assert
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void OK_Update_WhenCalled_ChangesNameAndEmail()
    {
        // Arrange
        var customer = new Customer("Old Name", Email.Create("old@example.com"));
        var newName = "New Name";
        var newEmail = Email.Create("new@example.com");

        // Act
        customer.Update(newName, newEmail);

        // Assert
        customer.Name.Should().Be(newName);
        customer.Email.Should().Be(newEmail);
        customer.Status.Should().Be(CustomerStatus.Active);
    }
    
    [Fact]
    public void NG_Deactivate_WhenAlreadyInactive_ThrowsDomainException()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", Email.Create("a@example.com"));
        customer.Deactivate();
        
        // Act
        var action = () => customer.Deactivate();

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Customer is already inactive.");
    }
    
    [Fact]
    public void NG_Reactivate_WhenAlreadyActive_ThrowsDomainException()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", Email.Create("a@example.com"));
        
        // Act
        var action = () => customer.Reactivate();

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Customer is already active.");
    }
}
