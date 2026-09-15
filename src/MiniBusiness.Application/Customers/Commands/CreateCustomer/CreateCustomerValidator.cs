using FluentValidation;

namespace MiniBusiness.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("The Name field must not be empty.")
            .MaximumLength(200)
                .WithMessage("Name cannot exceed 200 characters.");
        
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("The Email field must not be empty.")
            .EmailAddress()
                .WithMessage("The Email field is not a valid email address.")
            .MaximumLength(320)
                .WithMessage("Email cannot exceed 320 characters.");
    }
}