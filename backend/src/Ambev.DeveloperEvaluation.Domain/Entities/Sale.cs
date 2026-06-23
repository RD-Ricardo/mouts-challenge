using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public SaleItem AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId && !i.Cancelled);
        if (existing != null)
            throw new DomainException($"Product {productId} is already in this sale. Update the existing item instead.");

        var item = new SaleItem(productId, productName, quantity, unitPrice);
        _items.Add(item);
        Recalculate();
        return item;
    }

    public void ReplaceItems(IEnumerable<(Guid productId, string productName, int quantity, decimal unitPrice)> items)
    {
        _items.Clear();
        foreach (var (productId, productName, quantity, unitPrice) in items)
            AddItem(productId, productName, quantity, unitPrice);
    }

    public SaleItem CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found in sale {Id}.");
        if (item.Cancelled) return item;
        item.Cancel();
        Recalculate();
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void Cancel()
    {
        Cancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch() => UpdatedAt = DateTime.UtcNow;

    public void Recalculate()
    {
        TotalAmount = Math.Round(_items.Where(i => !i.Cancelled).Sum(i => i.TotalAmount), 2);
    }

    public ValidationResultDetail Validate()
    {
        var validator = new SaleValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
