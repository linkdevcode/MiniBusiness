using FluentValidation;

namespace MiniBusiness.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerValidator
    : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
                .WithMessage("Id cannot be empty");

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
                .WithMessage("Email cannot exceed 320 characters");
    }
}