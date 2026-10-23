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
            .When(x => x.Page.HasValue)
            .WithMessage($"Page must be greater than or equal to {minPage}.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(minPageSize, maxPageSize)
            .When(x => x.PageSize.HasValue)
            .WithMessage($"Page size must be between {minPageSize} and {maxPageSize}.");
    }
}
