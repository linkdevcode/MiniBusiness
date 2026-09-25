using FluentAssertions;
using MiniBusiness.Application.Customers.Commands.CreateCustomer;
using MiniBusiness.Application.Customers.Commands.DeactivateCustomer;
using MiniBusiness.Application.Customers.Commands.UpdateCustomer;
using MiniBusiness.Domain.Entities;
using MiniBusiness.Domain.Enums;
using MiniBusiness.UnitTests.Fakes;

namespace MiniBusiness.UnitTests.Application;

public class CustomerHandlerTests
{
    [Fact]
    public async Task OK_Create()
    {
        // Arrange
        var repository = new FakeCustomerRepository()
        {
            ExistsByEmailResult = false
        };
        var handler = new CreateCustomerHandler(repository);
        var command = new CreateCustomerCommand("Nguyen Van A", "a@example.com");
        
        // Act
        var customer = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be("Nguyen Van A");
        customer.Email.Should().Be("a@example.com");
        customer.Status.Should().Be(nameof(CustomerStatus.Active));
        repository.AddCallCount.Should().Be(1);
        repository.SaveChangesCallCount.Should().Be(1);
    }
    
    [Fact]
    public async Task NG_Create_WhenEmailExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = new FakeCustomerRepository()
        {
            ExistsByEmailResult = true
        };
        var handler = new CreateCustomerHandler(repository);
        var command = new CreateCustomerCommand("Nguyen Van A", "a@example.com");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A customer with this email already exists.");
        repository.AddCallCount.Should().Be(0);
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task OK_Update()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", "a@example.com");
        var repository = new FakeCustomerRepository()
        {
            CustomerById = customer,
            ExistsByEmailExceptResult = false
        };
        var handler = new UpdateCustomerHandler(repository);
        var command = new UpdateCustomerCommand(customer.Id,"Nguyen Van B", "b@example.com");
        
        // Act
        var customerUpdated = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        customerUpdated.Id.Should().Be(customer.Id);
        customerUpdated.Name.Should().Be("Nguyen Van B");
        customerUpdated.Email.Should().Be("b@example.com");
        repository.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NG_Update_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var repository = new FakeCustomerRepository()
        {
            CustomerById = null
        };
        var handler = new UpdateCustomerHandler(repository);
        var command = new UpdateCustomerCommand(Guid.NewGuid(), "Name", "name@example.com");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer was not found.");
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task NG_Update_WhenEmailExceptExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", "a@example.com");
        var repository = new FakeCustomerRepository()
        {
            CustomerById = customer,
            ExistsByEmailExceptResult = true
        };
        var handler = new UpdateCustomerHandler(repository);
        var command = new UpdateCustomerCommand(customer.Id, customer.Name, customer.Email);
        
        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A customer with this email already exists.");
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task OK_Deactivate()
    {
        // Arrange
        var customer = new Customer("Nguyen Van A", "a@example.com");
        var repository = new FakeCustomerRepository()
        {
            CustomerById = customer
        };
        var handler = new DeactivateCustomerHandler(repository);
        var command = new DeactivateCustomerCommand(customer.Id);
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        customer.Status.Should().Be(CustomerStatus.Inactive);
        repository.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NG_Deactivate_WhenCustomerNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var repository = new FakeCustomerRepository()
        {
            CustomerById = null
        };
        var handler = new DeactivateCustomerHandler(repository);
        var command = new DeactivateCustomerCommand(Guid.NewGuid());
        
        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer was not found.");
        repository.SaveChangesCallCount.Should().Be(0);
    }
}