using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _handler = new CreateSaleHandler(_repository, _mapper, _mediator);
    }

    private static CreateSaleCommand ValidCommand() => new()
    {
        SaleNumber = "S-100",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Customer X",
        BranchId = Guid.NewGuid(),
        BranchName = "Branch Y",
        Items = new List<CreateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "P1", Quantity = 5, UnitPrice = 10m }
        }
    };

    [Fact(DisplayName = "Valid command creates sale and publishes SaleCreatedNotification")]
    public async Task Handle_Valid_CreatesAndPublishes()
    {
        var command = ValidCommand();
        _repository.GetBySaleNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _repository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<SaleResult>(Arg.Any<Sale>())
            .Returns(new SaleResult { SaleNumber = command.SaleNumber });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _repository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(Arg.Any<SaleCreatedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Existing sale number throws InvalidOperationException")]
    public async Task Handle_DuplicateNumber_Throws()
    {
        var command = ValidCommand();
        _repository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(new Sale { SaleNumber = command.SaleNumber });

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Empty items fails validation")]
    public async Task Handle_NoItems_Throws()
    {
        var command = ValidCommand();
        command.Items.Clear();

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
