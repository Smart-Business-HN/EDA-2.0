using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.DiscountSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.DiscountFeature.Queries
{
    public class GetAllDiscountsQuery : IRequest<Result<PaginatedResult<Discount>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllDiscountsQueryHandler : IRequestHandler<GetAllDiscountsQuery, Result<PaginatedResult<Discount>>>
    {
        private readonly IRepositoryAsync<Discount> _repositoryAsync;

        public GetAllDiscountsQueryHandler(IRepositoryAsync<Discount> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Discount>>> Handle(GetAllDiscountsQuery request, CancellationToken cancellationToken)
        {
            List<Discount> discounts;
            int totalCount;

            if (request.GetAll)
            {
                // Obtener todos los descuentos sin paginación
                discounts = await _repositoryAsync.ListAsync(
                    new FilterDiscountsSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = discounts.Count;

                var allResult = new PaginatedResult<Discount>(discounts, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Discount>>(allResult);
            }
            else
            {
                // Obtener con paginación
                discounts = await _repositoryAsync.ListAsync(
                    new FilterDiscountsSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountDiscountsSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Discount>(discounts, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Discount>>(paginatedResult);
            }
        }
    }
}
