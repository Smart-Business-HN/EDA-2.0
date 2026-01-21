using EDA.APPLICATION.Repository;
using EDA.APPLICATION.Specifications.TaxSpecification;
using EDA.APPLICATION.Wrappers;
using EDA.DOMAIN.Entities;
using MediatR;

namespace EDA.APPLICATION.Features.TaxFeature.Queries
{
    public class GetAllTaxesQuery : IRequest<Result<PaginatedResult<Tax>>>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool GetAll { get; set; } = false;
    }

    public class GetAllTaxesQueryHandler : IRequestHandler<GetAllTaxesQuery, Result<PaginatedResult<Tax>>>
    {
        private readonly IRepositoryAsync<Tax> _repositoryAsync;

        public GetAllTaxesQueryHandler(IRepositoryAsync<Tax> repositoryAsync)
        {
            _repositoryAsync = repositoryAsync;
        }

        public async Task<Result<PaginatedResult<Tax>>> Handle(GetAllTaxesQuery request, CancellationToken cancellationToken)
        {
            List<Tax> taxes;
            int totalCount;

            if (request.GetAll)
            {
                taxes = await _repositoryAsync.ListAsync(
                    new FilterTaxesSpecification(request.SearchTerm),
                    cancellationToken);
                totalCount = taxes.Count;

                var allResult = new PaginatedResult<Tax>(taxes, totalCount, 1, totalCount > 0 ? totalCount : 1);
                return new Result<PaginatedResult<Tax>>(allResult);
            }
            else
            {
                taxes = await _repositoryAsync.ListAsync(
                    new FilterTaxesSpecification(request.SearchTerm, request.PageNumber, request.PageSize),
                    cancellationToken);

                totalCount = await _repositoryAsync.CountAsync(
                    new CountTaxesSpecification(request.SearchTerm),
                    cancellationToken);

                var paginatedResult = new PaginatedResult<Tax>(taxes, totalCount, request.PageNumber, request.PageSize);
                return new Result<PaginatedResult<Tax>>(paginatedResult);
            }
        }
    }
}
