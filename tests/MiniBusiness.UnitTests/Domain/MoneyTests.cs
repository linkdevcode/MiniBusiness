using FluentAssertions;
using MiniBusiness.Domain.Exceptions;
using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void OK_Create_ValidMoney()
    {
        // Arrange & Act
        var money = new Money(100.50m, "VND");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("VND");
    }
    
    [Fact]
    public void OK_Create_ZeroAmount()
    {
        // Arrange & Act
        var money = new Money(0m, "USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void NG_Create_NegativeAmount_ThrowsDomainException()
    {
        // Arrange & Act
        var action = () => new Money(-10.00m, "VND");
        
        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("The amount cannot be a negative number.");
    }
    
    [Fact]
    public void NG_Create_MissingCurrency_ThrowsDomainException()
    {
        // Arrange & Act
        var action = () => new Money(10.00m, string.Empty);
        
        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("The currency code must not be left blank.");
    }
    
    [Fact]
    public void OK_Add_SameCurrency()
    {
        // Arrange
        var m1 = new Money(100m, "VND");
        var m2 = new Money(250m, "VND");

        // Act
        var result = m1.Add(m2);

        // Assert
        result.Amount.Should().Be(350m);
        result.Currency.Should().Be("VND");
    }
    
    [Fact]
    public void NG_Add_DifferentCurrency_ThrowsDomainException()
    {
        // Arrange
        var m1 = new Money(100m, "VND");
        var m2 = new Money(50m, "USD");

        // Act
        var action = () => m1.Add(m2);
        
        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(
                $"Cannot perform an operation between two different currencies: {m1.Currency} and {m2.Currency}");
    }
    
    [Fact]
    public void OK_Equality_SameAmountAndCurrency()
    {
        // Arrange
        var m1 = new Money(500m, "VND");
        var m2 = new Money(500m, "VND");

        // Assert
        m1.Should().Be(m2);
    }

    [Fact]
    public void NG_Equality_DifferentAmountOrCurrency()
    {
        // Arrange
        var m1 = new Money(500m, "VND");
        var m2 = new Money(500m, "USD");
        var m3 = new Money(600m, "VND");

        // Assert
        m1.Should().NotBe(m2);
        m1.Should().NotBe(m3);
        m2.Should().NotBe(m3);
    }
}