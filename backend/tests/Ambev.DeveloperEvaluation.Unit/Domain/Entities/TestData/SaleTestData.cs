using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static Sale GenerateValidSale(int itemCount = 1, int quantityPerItem = 1)
    {
        var sale = new Sale
        {
            SaleNumber = $"S{Faker.Random.Number(1000, 999999)}",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName()
        };

        for (var i = 0; i < itemCount; i++)
            sale.AddItem(Guid.NewGuid(), Faker.Commerce.ProductName(), quantityPerItem, Faker.Random.Decimal(1m, 100m));

        return sale;
    }
}
