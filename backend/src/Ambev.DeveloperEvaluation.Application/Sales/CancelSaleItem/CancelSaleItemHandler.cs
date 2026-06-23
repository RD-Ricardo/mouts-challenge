using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CancelSaleItemHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found.");

        var item = sale.CancelItem(request.ItemId);
        var updated = await _repository.UpdateAsync(sale, cancellationToken);
        
        await _mediator.Publish(new ItemCancelledNotification(new ItemCancelledEvent(updated.Id, item.Id, item.ProductId, item.ProductName)), cancellationToken);
        return _mapper.Map<SaleResult>(updated);
    }
}
