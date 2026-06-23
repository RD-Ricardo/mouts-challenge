using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        RuleFor(x => x.SaleDate).NotEqual(default(DateTime));
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchName).NotEmpty();
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
