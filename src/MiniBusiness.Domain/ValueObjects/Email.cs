using System.Net.Mail;
using MiniBusiness.Domain.Exceptions;

namespace MiniBusiness.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }
    
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        return !IsValidEmail(normalizedEmail) 
            ? throw new DomainException("Email format is invalid.") 
            : new Email(normalizedEmail);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    public static implicit operator string(Email email) => email.Value;
    
    public override string ToString() => Value;
}