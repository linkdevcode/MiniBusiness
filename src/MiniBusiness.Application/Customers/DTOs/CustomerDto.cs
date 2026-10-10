namespace MiniBusiness.Application.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
    );