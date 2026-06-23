using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _handler = new CancelSaleHandler(_repository, _mapper, _mediator);
    }

    [Fact(DisplayName = "Cancelling existing sale marks it cancelled and publishes event")]
    public async Task Handle_Existing_CancelsAndPublishes()
    {
        var sale = SaleTestData.GenerateValidSale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleResult>(sale).Returns(new SaleResult { Id = sale.Id, Cancelled = true });

        var result = await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        sale.Cancelled.Should().BeTrue();
        result.Cancelled.Should().BeTrue();
        await _mediator.Received(1).Publish(Arg.Any<SaleCancelledNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Cancelling missing sale throws KeyNotFoundException")]
    public async Task Handle_Missing_Throws()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var act = () => _handler.Handle(new CancelSaleCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
