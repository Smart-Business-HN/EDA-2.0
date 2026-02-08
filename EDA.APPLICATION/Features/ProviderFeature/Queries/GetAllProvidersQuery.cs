using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.ProviderSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.ProviderFeature.Queries
{
    public class GetAllProvidersQuery : IRequest<Result<PaginatedResult<Provider>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, Result<PaginatedResult<Provider>>>
    {
        private readonly IRepositoryAsync<Provider> _repositoryAsync;

        public GetAllProvidersQueryHandler(IRepositoryAsync<Provider> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Provider>>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
        {
            List<Provider> providers;
            int totalCount;

            if (request.GetAll)
            {
                providers = await _repositoryAsync.ListAsync(
                    new FilterProvidersSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = providers.Count;

                var allResult = new PaginatedResult<Provider>(providers, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Provider>>(allResult);
            }
            else
            {
                providers = await _repositoryAsync.ListAsync(
                    new FilterProvidersSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountProvidersSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Provider>(providers, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Provider>>(paginatedResult);
            }
        }
    }
}
