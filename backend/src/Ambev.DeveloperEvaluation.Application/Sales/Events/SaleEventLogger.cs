using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleEventLogger :
    INotificationHandler<SaleCreatedNotification>,
    INotificationHandler<SaleModifiedNotification>,
    INotificationHandler<SaleCancelledNotification>,
    INotificationHandler<ItemCancelledNotification>
{
    private readonly ILogger<SaleEventLogger> _logger;

    public SaleEventLogger(ILogger<SaleEventLogger> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedNotification n, CancellationToken ct)
    {
        _logger.LogInformation("SaleCreated: SaleId={SaleId} Number={Number} Total={Total} Items={Items}",
            n.Event.Sale.Id, n.Event.Sale.SaleNumber, n.Event.Sale.TotalAmount, n.Event.Sale.Items.Count);
        return Task.CompletedTask;
    }

    public Task Handle(SaleModifiedNotification n, CancellationToken ct)
    {
        _logger.LogInformation("SaleModified: SaleId={SaleId} Number={Number} Total={Total}",
            n.Event.Sale.Id, n.Event.Sale.SaleNumber, n.Event.Sale.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(SaleCancelledNotification n, CancellationToken ct)
    {
        _logger.LogInformation("SaleCancelled: SaleId={SaleId} Number={Number}",
            n.Event.Sale.Id, n.Event.Sale.SaleNumber);
        return Task.CompletedTask;
    }

    public Task Handle(ItemCancelledNotification n, CancellationToken ct)
    {
        _logger.LogInformation("ItemCancelled: SaleId={SaleId} ItemId={ItemId} ProductId={ProductId} Product={Product}",
            n.Event.SaleId, n.Event.ItemId, n.Event.ProductId, n.Event.ProductName);
        return Task.CompletedTask;
    }
}
