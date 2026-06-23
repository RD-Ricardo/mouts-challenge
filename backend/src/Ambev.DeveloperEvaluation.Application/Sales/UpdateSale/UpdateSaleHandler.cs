using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateSaleHandler(ISaleRepository repository, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        sale.SaleDate = command.SaleDate;
        sale.CustomerId = command.CustomerId;
        sale.CustomerName = command.CustomerName;
        sale.BranchId = command.BranchId;
        sale.BranchName = command.BranchName;
        sale.ReplaceItems(command.Items.Select(i => (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)));
        sale.Touch();

        var updated = await _repository.UpdateAsync(sale, cancellationToken);
        await _mediator.Publish(new SaleModifiedNotification(new SaleModifiedEvent(updated)), cancellationToken);
        return _mapper.Map<SaleResult>(updated);
    }
}
