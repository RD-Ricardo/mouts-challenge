using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Theory(DisplayName = "Quantities below 4 receive no discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Item_QuantitiesBelowFour_NoDiscount(int qty)
    {
        var item = new SaleItem(Guid.NewGuid(), "Product", qty, 10m);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(10m * qty);
    }

    [Theory(DisplayName = "Quantities between 4 and 9 get 10% discount")]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void Item_QuantitiesFourToNine_TenPercentDiscount(int qty)
    {
        var item = new SaleItem(Guid.NewGuid(), "Product", qty, 100m);
        item.Discount.Should().Be(qty * 100m * 0.10m);
        item.TotalAmount.Should().Be(qty * 100m * 0.90m);
    }

    [Theory(DisplayName = "Quantities between 10 and 20 get 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Item_QuantitiesTenToTwenty_TwentyPercentDiscount(int qty)
    {
        var item = new SaleItem(Guid.NewGuid(), "Product", qty, 50m);
        item.Discount.Should().Be(qty * 50m * 0.20m);
        item.TotalAmount.Should().Be(qty * 50m * 0.80m);
    }

    [Theory(DisplayName = "Quantities above 20 throw DomainException")]
    [InlineData(21)]
    [InlineData(50)]
    public void Item_QuantityAboveTwenty_Throws(int qty)
    {
        var act = () => new SaleItem(Guid.NewGuid(), "Product", qty, 10m);
        act.Should().Throw<DomainException>().WithMessage("*more than 20*");
    }

    [Fact(DisplayName = "Quantity less than one throws DomainException")]
    public void Item_QuantityLessThanOne_Throws()
    {
        var act = () => new SaleItem(Guid.NewGuid(), "Product", 0, 10m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Unit price must be greater than zero")]
    public void Item_NonPositivePrice_Throws()
    {
        var act = () => new SaleItem(Guid.NewGuid(), "Product", 1, 0m);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Sale total equals sum of non-cancelled items")]
    public void Sale_TotalAmount_AggregatesItems()
    {
        var sale = new Sale
        {
            SaleNumber = "S1",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B"
        };
        sale.AddItem(Guid.NewGuid(), "P1", 2, 10m); // 20
        sale.AddItem(Guid.NewGuid(), "P2", 5, 100m); // 5*100*0.9 = 450
        sale.TotalAmount.Should().Be(470m);
    }

    [Fact(DisplayName = "Cancelling an item recalculates sale total")]
    public void Sale_CancelItem_RecalculatesTotal()
    {
        var sale = new Sale
        {
            SaleNumber = "S1",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B"
        };
        var item1 = sale.AddItem(Guid.NewGuid(), "P1", 2, 10m);
        sale.AddItem(Guid.NewGuid(), "P2", 1, 30m);
        sale.TotalAmount.Should().Be(50m);

        sale.CancelItem(item1.Id);
        sale.TotalAmount.Should().Be(30m);
        item1.Cancelled.Should().BeTrue();
        item1.TotalAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "Cancelling unknown item throws KeyNotFoundException")]
    public void Sale_CancelUnknownItem_Throws()
    {
        var sale = new Sale
        {
            SaleNumber = "S1",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B"
        };
        var act = () => sale.CancelItem(Guid.NewGuid());
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Cancelling sale flags Cancelled true")]
    public void Sale_Cancel_SetsFlag()
    {
        var sale = new Sale
        {
            SaleNumber = "S1",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B"
        };
        sale.AddItem(Guid.NewGuid(), "P1", 1, 10m);
        sale.Cancel();
        sale.Cancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Adding a duplicate product throws DomainException")]
    public void Sale_DuplicateProduct_Throws()
    {
        var sale = new Sale
        {
            SaleNumber = "S1",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "C",
            BranchId = Guid.NewGuid(),
            BranchName = "B"
        };
        var productId = Guid.NewGuid();
        sale.AddItem(productId, "P1", 1, 10m);
        var act = () => sale.AddItem(productId, "P1", 2, 10m);
        act.Should().Throw<DomainException>();
    }
}
