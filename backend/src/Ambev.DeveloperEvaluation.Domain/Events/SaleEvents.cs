using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record SaleCreatedEvent(Sale Sale);
public record SaleModifiedEvent(Sale Sale);
public record SaleCancelledEvent(Sale Sale);
public record ItemCancelledEvent(Guid SaleId, Guid ItemId, Guid ProductId, string ProductName);
