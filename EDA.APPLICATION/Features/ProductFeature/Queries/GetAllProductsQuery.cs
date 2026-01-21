using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProductSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProductFeature.Queries
{
    public class GetAllProductsQuery : IRequest<Result<PaginatedResult<Product>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PaginatedResult<Product>>>
    {
        private readonly IRepositoryAsync<Product> _repositoryAsync;

        public GetAllProductsQueryHandler(IRepositoryAsync<Product> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Product>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            List<Product> products;
            int totalCount;

            if (request.GetAll)
            {
                products = await _repositoryAsync.ListAsync(
                    new FilterProductsSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = products.Count;

                var allResult = new PaginatedResult<Product>(products, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Product>>(allResult);
            }
            else
            {
                products = await _repositoryAsync.ListAsync(
                    new FilterProductsSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountProductsSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Product>(products, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Product>>(paginatedResult);
            }
        }
    }
}
