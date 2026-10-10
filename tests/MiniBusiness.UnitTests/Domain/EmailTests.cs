using FluentAssertions;
using MiniBusiness.Domain.Exceptions;
using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.UnitTests.Domain;

public class EmailTests
{
    [Fact]
    public void OK_CreateEmail()
    {
        // Arrange
        var emailValue = "a@example.com";

        // Act
        var email = Email.Create(emailValue);
        
        // Assert
        email.Value.Should().Be(emailValue);
        email.ToString().Should().Be(emailValue);
    }

    [Fact]
    public void NG_CreateEmailWithEmptyValue()
    {
        // Act
        var action = () => Email.Create(string.Empty);
        
        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Email is required.");
    }
    
    [Fact]
    public void NG_CreateEmailWithInvalidFormat()
    {
        // Arrange
        var emailValue = "aexample.com";

        // Act
        var action = () => Email.Create(emailValue);
        
        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Email format is invalid.");
    }
}