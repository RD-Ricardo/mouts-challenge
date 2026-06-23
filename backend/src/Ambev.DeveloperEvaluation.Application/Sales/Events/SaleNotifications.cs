using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public record SaleCreatedNotification(SaleCreatedEvent Event) : INotification;
public record SaleModifiedNotification(SaleModifiedEvent Event) : INotification;
public record SaleCancelledNotification(SaleCancelledEvent Event) : INotification;
public record ItemCancelledNotification(ItemCancelledEvent Event) : INotification;
