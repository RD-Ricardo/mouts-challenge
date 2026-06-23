using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; private set; }

    private SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        Recalculate();
    }

    public void Update(int quantity, decimal unitPrice, string productName)
    {
        ProductName = productName;
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        Recalculate();
    }

    public void Cancel()
    {
        Cancelled = true;
        TotalAmount = 0m;
        Discount = 0m;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity < 1)
            throw new DomainException("Item quantity must be at least 1.");
        if (quantity > 20)
            throw new DomainException("It is not possible to sell more than 20 identical items.");
        Quantity = quantity;
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0m)
            throw new DomainException("Unit price must be greater than zero.");
        UnitPrice = unitPrice;
    }

    private void Recalculate()
    {
        if (Cancelled)
        {
            TotalAmount = 0m;
            Discount = 0m;
            return;
        }

        var gross = UnitPrice * Quantity;
        var rate = DiscountRateFor(Quantity);
        Discount = Math.Round(gross * rate, 2);
        TotalAmount = Math.Round(gross - Discount, 2);
    }

    public static decimal DiscountRateFor(int quantity)
    {
        if (quantity >= 10 && quantity <= 20) return 0.20m;
        if (quantity >= 4) return 0.10m;
        return 0m;
    }
}
