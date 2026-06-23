using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SaleDate).NotEqual(default(DateTime));
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(p => p.ProductId).NotEqual(Guid.Empty);
            i.RuleFor(p => p.ProductName).NotEmpty();
            i.RuleFor(p => p.Quantity).InclusiveBetween(1, 20);
            i.RuleFor(p => p.UnitPrice).GreaterThan(0m);
        });
    }
}
