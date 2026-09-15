namespace MiniBusiness.Api.Requests;

public sealed record UpdateCustomerRequest(
    string Name,
    string Email
);