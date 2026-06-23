using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _handler = new CancelSaleItemHandler(_repository, _mapper, _mediator);
    }

    [Fact(DisplayName = "Cancelling item recalculates total and publishes ItemCancelledNotification")]
    public async Task Handle_Valid_RecalculatesAndPublishes()
    {
        var sale = SaleTestData.GenerateValidSale(itemCount: 2, quantityPerItem: 1);
        var itemToCancel = sale.Items.First();
        var originalTotal = sale.TotalAmount;
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleResult>(sale).Returns(new SaleResult { Id = sale.Id });

        await _handler.Handle(new CancelSaleItemCommand(sale.Id, itemToCancel.Id), CancellationToken.None);

        itemToCancel.Cancelled.Should().BeTrue();
        sale.TotalAmount.Should().BeLessThan(originalTotal);
        await _mediator.Received(1).Publish(Arg.Any<ItemCancelledNotification>(), Arg.Any<CancellationToken>());
    }
}
