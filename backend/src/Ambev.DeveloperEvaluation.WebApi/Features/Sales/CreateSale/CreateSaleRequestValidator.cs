using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SaleDate).NotEqual(default(DateTime));
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
