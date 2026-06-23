using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var size = request.Size < 1 ? 10 : request.Size;

        var (items, total) = await _repository.ListAsync(page, size, request.Order, request.Filters, cancellationToken);

        return new ListSalesResult
        {
            Items = items.Select(_mapper.Map<SaleResult>).ToList(),
            TotalCount = total,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }
}
