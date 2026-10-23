using FluentValidation;
using Microsoft.Extensions.Options;
using MiniBusiness.Application.Common.Options;

namespace MiniBusiness.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var minPage = paginationOptions.Value.MinPage;
        var minPageSize = paginationOptions.Value.MinPageSize;
        var maxPageSize = paginationOptions.Value.MaxPageSize;
        
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(minPage)
            .When(x => x.Page >= minPage)
            .WithMessage($"Page must be greater than or equal to {minPage}.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(minPageSize)
            .When(x => x.PageSize >= minPageSize)
            .WithMessage($"Page size must be greater than or equal to {minPageSize}.")
            .LessThanOrEqualTo(maxPageSize)
            .When(x => x.PageSize <= maxPageSize)
            .WithMessage($"Page size must not exceed {maxPageSize}");
    }
}